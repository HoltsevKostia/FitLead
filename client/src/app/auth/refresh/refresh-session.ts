import "server-only";

import { createHash } from "node:crypto";
import type { NextRequest } from "next/server";

import { getSetCookieHeaders } from "@/app/api/auth/cookie-utils";
import { appendForwardedHeadersFromRequest } from "@/lib/api/forwarded-headers";
import { serverApiEnv } from "@/lib/api/server-env";

const csrfCookieName = "FitLead.XSRF-TOKEN";
const csrfHeaderName = "X-CSRF-TOKEN";
const refreshTokenCookieName = "fitlead.refresh_token";
const completedResultTtlMs = 10_000;

interface RefreshSessionResult {
  success: boolean;
  status: number;
  csrfSetCookieHeaders: string[];
  refreshSetCookieHeaders: string[];
}

interface RefreshEntry {
  promise: Promise<RefreshSessionResult>;
}

const refreshEntries = new Map<string, RefreshEntry>();

function buildBackendUrl(path: string): string {
  return new URL(path, serverApiEnv.baseUrl).toString();
}

function getCookieValueFromSetCookie(
  setCookie: string,
  cookieName: string,
): string | null {
  const [cookiePair] = setCookie.split(";");
  const separatorIndex = cookiePair.indexOf("=");

  if (separatorIndex < 0) {
    return null;
  }

  const name = cookiePair.slice(0, separatorIndex).trim();
  if (name !== cookieName) {
    return null;
  }

  return decodeURIComponent(cookiePair.slice(separatorIndex + 1));
}

function mergeCookieHeader(
  currentCookieHeader: string,
  setCookieHeaders: string[],
): string {
  const cookies = new Map<string, string>();

  for (const cookie of currentCookieHeader.split(";")) {
    const trimmedCookie = cookie.trim();
    const separatorIndex = trimmedCookie.indexOf("=");

    if (separatorIndex <= 0) {
      continue;
    }

    cookies.set(
      trimmedCookie.slice(0, separatorIndex),
      trimmedCookie.slice(separatorIndex + 1),
    );
  }

  for (const setCookie of setCookieHeaders) {
    const [cookiePair] = setCookie.split(";");
    const separatorIndex = cookiePair.indexOf("=");

    if (separatorIndex <= 0) {
      continue;
    }

    cookies.set(
      cookiePair.slice(0, separatorIndex).trim(),
      cookiePair.slice(separatorIndex + 1),
    );
  }

  return [...cookies.entries()]
    .map(([name, value]) => `${name}=${value}`)
    .join("; ");
}

function getRefreshKey(refreshToken: string): string {
  return createHash("sha256").update(refreshToken).digest("base64url");
}

async function performRefresh(
  request: NextRequest,
  cookieHeader: string,
): Promise<RefreshSessionResult> {
  const csrfHeaders = new Headers();
  appendForwardedHeadersFromRequest(csrfHeaders, request);
  if (cookieHeader) {
    csrfHeaders.set("Cookie", cookieHeader);
  }

  const csrfResponse = await fetch(buildBackendUrl("/auth/csrf-token"), {
    method: "GET",
    headers: csrfHeaders,
    cache: "no-store",
  });
  const csrfSetCookieHeaders = getSetCookieHeaders(csrfResponse);
  const csrfToken =
    csrfResponse.ok
      ? csrfSetCookieHeaders
          .map((setCookie) =>
            getCookieValueFromSetCookie(setCookie, csrfCookieName),
          )
          .find((value): value is string => Boolean(value)) ?? null
      : null;

  if (!csrfToken) {
    return {
      success: false,
      status: csrfResponse.status,
      csrfSetCookieHeaders,
      refreshSetCookieHeaders: [],
    };
  }

  const refreshHeaders = new Headers({
    Accept: "application/json, application/problem+json",
    Cookie: mergeCookieHeader(cookieHeader, csrfSetCookieHeaders),
    [csrfHeaderName]: csrfToken,
  });
  appendForwardedHeadersFromRequest(refreshHeaders, request);

  const refreshResponse = await fetch(buildBackendUrl("/auth/refresh"), {
    method: "POST",
    headers: refreshHeaders,
    cache: "no-store",
  });

  return {
    success: refreshResponse.ok,
    status: refreshResponse.status,
    csrfSetCookieHeaders,
    refreshSetCookieHeaders: getSetCookieHeaders(refreshResponse),
  };
}

export function refreshSession(
  request: NextRequest,
): Promise<RefreshSessionResult> {
  const refreshToken = request.cookies.get(refreshTokenCookieName)?.value;
  if (!refreshToken) {
    return Promise.resolve({
      success: false,
      status: 401,
      csrfSetCookieHeaders: [],
      refreshSetCookieHeaders: [],
    });
  }

  const key = getRefreshKey(refreshToken);
  const existingEntry = refreshEntries.get(key);
  if (existingEntry) {
    return existingEntry.promise;
  }

  const entry: RefreshEntry = {
    promise: performRefresh(
      request,
      request.headers.get("cookie") ?? "",
    ),
  };
  refreshEntries.set(key, entry);

  function scheduleCleanup() {
    const timeout = setTimeout(() => {
      if (refreshEntries.get(key) === entry) {
        refreshEntries.delete(key);
      }
    }, completedResultTtlMs);

    timeout.unref();
  }

  void entry.promise.then(scheduleCleanup, scheduleCleanup);

  return entry.promise;
}
