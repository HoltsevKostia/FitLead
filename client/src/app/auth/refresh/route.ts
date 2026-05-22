import { type NextRequest, NextResponse } from "next/server";

import { apiEnv } from "@/lib/api/env";
import { resolveSafeNextHref } from "@/shared/utils/resolve-safe-next-href";

export const runtime = "nodejs";

const csrfCookieName = "FitLead.XSRF-TOKEN";
const csrfHeaderName = "X-CSRF-TOKEN";
const defaultNextHref = "/dashboard";

function buildBackendUrl(path: string): string {
  return new URL(path, apiEnv.baseUrl).toString();
}

function getSetCookieHeaders(response: Response): string[] {
  const headersWithGetSetCookie = response.headers as Headers & {
    getSetCookie?: () => string[];
  };

  if (!headersWithGetSetCookie.getSetCookie) {
    throw new Error("getSetCookie is not available in this runtime.");
  }

  return headersWithGetSetCookie.getSetCookie();
}

function appendSetCookieHeaders(response: NextResponse, setCookieHeaders: string[]) {
  for (const setCookie of setCookieHeaders) {
    response.headers.append("Set-Cookie", setCookie);
  }
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

function buildLoginRedirectUrl(request: NextRequest, nextHref: string): URL {
  const url = request.nextUrl.clone();
  url.pathname = "/login";
  url.search = "";
  url.searchParams.set("next", nextHref);
  return url;
}

function buildSafeRedirectUrl(request: NextRequest, href: string): URL {
  return new URL(href, request.nextUrl.origin);
}

async function issueCsrfCookies(cookieHeader: string): Promise<{
  csrfToken: string | null;
  setCookieHeaders: string[];
}> {
  const response = await fetch(buildBackendUrl("/auth/csrf-token"), {
    method: "GET",
    headers: cookieHeader ? { Cookie: cookieHeader } : undefined,
    cache: "no-store",
  });

  const setCookieHeaders = getSetCookieHeaders(response);
  if (!response.ok) {
    return { csrfToken: null, setCookieHeaders };
  }

  const csrfToken =
    setCookieHeaders
      .map((setCookie) => getCookieValueFromSetCookie(setCookie, csrfCookieName))
      .find((value): value is string => Boolean(value)) ?? null;

  return { csrfToken, setCookieHeaders };
}

export async function GET(request: NextRequest) {
  const nextHref = resolveSafeNextHref(
    request.nextUrl.searchParams.get("next") ?? undefined,
    defaultNextHref,
  );
  const cookieHeader = request.headers.get("cookie") ?? "";

  try {
    const csrf = await issueCsrfCookies(cookieHeader);

    if (!csrf.csrfToken) {
      const response = NextResponse.redirect(buildLoginRedirectUrl(request, nextHref));
      appendSetCookieHeaders(response, csrf.setCookieHeaders);
      return response;
    }

    const refreshCookieHeader = mergeCookieHeader(cookieHeader, csrf.setCookieHeaders);
    const refreshResponse = await fetch(buildBackendUrl("/auth/refresh"), {
      method: "POST",
      headers: {
        Accept: "application/json, application/problem+json",
        Cookie: refreshCookieHeader,
        [csrfHeaderName]: csrf.csrfToken,
      },
      cache: "no-store",
    });
    const refreshSetCookieHeaders = getSetCookieHeaders(refreshResponse);

    if (!refreshResponse.ok) {
      const response = NextResponse.redirect(buildLoginRedirectUrl(request, nextHref));
      appendSetCookieHeaders(response, csrf.setCookieHeaders);
      appendSetCookieHeaders(response, refreshSetCookieHeaders);
      return response;
    }

    const response = NextResponse.redirect(buildSafeRedirectUrl(request, nextHref));
    appendSetCookieHeaders(response, csrf.setCookieHeaders);
    appendSetCookieHeaders(response, refreshSetCookieHeaders);
    return response;
  } catch {
    const response = NextResponse.redirect(buildLoginRedirectUrl(request, nextHref));
    return response;
  }
}
