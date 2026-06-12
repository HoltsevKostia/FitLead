import { type NextRequest, NextResponse } from "next/server";

function getFirstHeaderValue(value: string | null): string | null {
  const firstValue = value?.split(",", 1)[0]?.trim();
  return firstValue || null;
}

function resolvePublicOrigin(request: NextRequest): string {
  const forwardedProto = getFirstHeaderValue(
    request.headers.get("x-forwarded-proto"),
  );
  const protocol =
    forwardedProto ??
    (process.env.NODE_ENV === "production"
      ? "https"
      : request.nextUrl.protocol.replace(/:$/, ""));
  const host =
    getFirstHeaderValue(request.headers.get("x-forwarded-host")) ??
    getFirstHeaderValue(request.headers.get("host"));

  if (!host) {
    throw new Error("Cannot resolve the public request host.");
  }

  return `${protocol}://${host}`;
}

export function createInternalRedirect(
  request: NextRequest,
  location: string,
): NextResponse {
  if (!location.startsWith("/") || location.startsWith("//")) {
    throw new Error("Relative redirect location must be an internal path.");
  }

  return NextResponse.redirect(new URL(location, resolvePublicOrigin(request)), 307);
}
