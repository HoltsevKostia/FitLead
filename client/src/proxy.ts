import { NextResponse, type NextRequest } from "next/server";

import { createRelativeRedirect } from "@/shared/utils/create-relative-redirect";
import { resolveSafeNextHref } from "@/shared/utils/resolve-safe-next-href";

const accessTokenCookieName = "fitlead.access_token";
const refreshTokenCookieName = "fitlead.refresh_token";
const accessTokenRefreshThresholdSeconds = 60;

function getCurrentHref(request: NextRequest): string {
  return resolveSafeNextHref(
    `${request.nextUrl.pathname}${request.nextUrl.search}`,
    "/dashboard",
  );
}

function buildAuthRedirectHref(path: "/login" | "/auth/refresh", nextHref: string) {
  return `${path}?next=${encodeURIComponent(nextHref)}`;
}

function decodeBase64Url(value: string): string {
  const base64 = value
    .replaceAll("-", "+")
    .replaceAll("_", "/")
    .padEnd(Math.ceil(value.length / 4) * 4, "=");

  return atob(base64);
}

function getJwtExpirationSeconds(token: string): number | null {
  const [, payload] = token.split(".");
  if (!payload) {
    return null;
  }

  try {
    const decodedPayload = JSON.parse(decodeBase64Url(payload)) as {
      exp?: unknown;
    };

    return typeof decodedPayload.exp === "number" ? decodedPayload.exp : null;
  } catch {
    return null;
  }
}

function isAccessTokenFresh(token: string, nowSeconds: number): boolean {
  const expiresAtSeconds = getJwtExpirationSeconds(token);
  if (!expiresAtSeconds) {
    return false;
  }

  return expiresAtSeconds - nowSeconds > accessTokenRefreshThresholdSeconds;
}

export function proxy(request: NextRequest) {
  const accessToken = request.cookies.get(accessTokenCookieName)?.value;
  const refreshToken = request.cookies.get(refreshTokenCookieName)?.value;
  const nowSeconds = Math.floor(Date.now() / 1000);

  if (accessToken && isAccessTokenFresh(accessToken, nowSeconds)) {
    return NextResponse.next();
  }

  const nextHref = getCurrentHref(request);
  const authPath = refreshToken ? "/auth/refresh" : "/login";

  return createRelativeRedirect(buildAuthRedirectHref(authPath, nextHref));
}

export const config = {
  matcher: [
    "/dashboard/:path*",
    "/chats/:path*",
    "/client/:path*",
    "/clients/:path*",
    "/exercises/:path*",
    "/invitations/:path*",
    "/training-programs/:path*",
    "/workouts/:path*",
  ],
};
