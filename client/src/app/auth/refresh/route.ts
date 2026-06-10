import { type NextRequest, NextResponse } from "next/server";

import {
  appendAuthSetCookieHeaders,
  appendClearAuthCookieHeaders,
  getSetCookieHeaders,
} from "@/app/api/auth/cookie-utils";
import { appendForwardedHeadersFromRequest } from "@/lib/api/forwarded-headers";
import { serverApiEnv } from "@/lib/api/server-env";
import { createRelativeRedirect } from "@/shared/utils/create-relative-redirect";
import { resolveSafeNextHref } from "@/shared/utils/resolve-safe-next-href";

export const runtime = "nodejs";

const csrfCookieName = "FitLead.XSRF-TOKEN";
const csrfHeaderName = "X-CSRF-TOKEN";
const defaultNextHref = "/dashboard";

function buildBackendUrl(path: string): string {
  return new URL(path, serverApiEnv.baseUrl).toString();
}

function withNoStore(response: NextResponse): NextResponse {
  response.headers.set("Cache-Control", "no-store");
  return response;
}

function getCookieValueFromSetCookie(setCookie: string, cookieName: string): string | null {
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

function buildLoginRedirectHref(nextHref: string): string {
  return `/login?next=${encodeURIComponent(nextHref)}`;
}

export async function GET(request: NextRequest) {
  const nextHref = resolveSafeNextHref(
    request.nextUrl.searchParams.get("next") ?? undefined,
    defaultNextHref,
  );
  const cookieHeader = request.headers.get("cookie") ?? "";

  try {
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
            .map((setCookie) => getCookieValueFromSetCookie(setCookie, csrfCookieName))
            .find((value): value is string => Boolean(value)) ?? null
        : null;

    const csrf = {
      csrfToken,
      setCookieHeaders: csrfSetCookieHeaders,
    };

    if (!csrf.csrfToken) {
      const response = createRelativeRedirect(buildLoginRedirectHref(nextHref));
      appendAuthSetCookieHeaders(response, csrf.setCookieHeaders);
      return withNoStore(response);
    }

    const refreshCookieHeader = mergeCookieHeader(cookieHeader, csrf.setCookieHeaders);
    const refreshHeaders = new Headers({
      Accept: "application/json, application/problem+json",
      Cookie: refreshCookieHeader,
      [csrfHeaderName]: csrf.csrfToken,
    });
    appendForwardedHeadersFromRequest(refreshHeaders, request);

    const refreshResponse = await fetch(buildBackendUrl("/auth/refresh"), {
      method: "POST",
      headers: refreshHeaders,
      cache: "no-store",
    });
    const refreshSetCookieHeaders = getSetCookieHeaders(refreshResponse);

    if (!refreshResponse.ok) {
      const response = createRelativeRedirect(buildLoginRedirectHref(nextHref));
      appendAuthSetCookieHeaders(response, csrf.setCookieHeaders);
      appendAuthSetCookieHeaders(response, refreshSetCookieHeaders);
      if (refreshResponse.status === 401 || refreshResponse.status === 403) {
        appendClearAuthCookieHeaders(response);
      }
      return withNoStore(response);
    }

    const response = createRelativeRedirect(nextHref);
    appendAuthSetCookieHeaders(response, csrf.setCookieHeaders);
    appendAuthSetCookieHeaders(response, refreshSetCookieHeaders);
    return withNoStore(response);
  } catch {
    const response = createRelativeRedirect(buildLoginRedirectHref(nextHref));
    return withNoStore(response);
  }
}
