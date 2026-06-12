import { type NextRequest } from "next/server";

import {
  appendAuthSetCookieHeaders,
  appendClearAuthCookieHeaders,
} from "@/app/api/auth/cookie-utils";
import { refreshSession } from "@/app/auth/refresh/refresh-session";
import { createInternalRedirect } from "@/shared/utils/create-internal-redirect";
import { resolveSafeNextHref } from "@/shared/utils/resolve-safe-next-href";

export const runtime = "nodejs";

const defaultNextHref = "/dashboard";

function withNoStore(response: Response): Response {
  response.headers.set("Cache-Control", "no-store");
  return response;
}

function buildLoginRedirectHref(nextHref: string): string {
  return `/login?next=${encodeURIComponent(nextHref)}`;
}

export async function GET(request: NextRequest) {
  const nextHref = resolveSafeNextHref(
    request.nextUrl.searchParams.get("next") ?? undefined,
    defaultNextHref,
  );

  try {
    const refresh = await refreshSession(request);

    if (!refresh.success) {
      const response = createInternalRedirect(
        request,
        buildLoginRedirectHref(nextHref),
      );
      appendAuthSetCookieHeaders(response, refresh.csrfSetCookieHeaders);
      appendAuthSetCookieHeaders(response, refresh.refreshSetCookieHeaders);
      if (refresh.status === 401 || refresh.status === 403) {
        appendClearAuthCookieHeaders(response);
      }
      return withNoStore(response);
    }

    const response = createInternalRedirect(request, nextHref);
    appendAuthSetCookieHeaders(response, refresh.csrfSetCookieHeaders);
    appendAuthSetCookieHeaders(response, refresh.refreshSetCookieHeaders);
    return withNoStore(response);
  } catch {
    const response = createInternalRedirect(
      request,
      buildLoginRedirectHref(nextHref),
    );
    return withNoStore(response);
  }
}
