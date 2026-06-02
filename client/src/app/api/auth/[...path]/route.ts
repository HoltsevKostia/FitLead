import { NextResponse, type NextRequest } from "next/server";

import { appendAuthSetCookieHeaders, getSetCookieHeaders } from "@/app/api/auth/cookie-utils";
import { appendForwardedHeadersFromRequest } from "@/lib/api/forwarded-headers";
import { serverApiEnv } from "@/lib/api/server-env";

export const runtime = "nodejs";

type RouteContext = {
  params: Promise<{
    path: string[];
  }>;
};

const noBodyMethods = new Set(["GET", "HEAD"]);
const noResponseBodyStatuses = new Set([204, 304]);
const allowedAuthPaths = new Set([
  "login",
  "register",
  "logout",
  "refresh",
  "csrf-token",
  "current-user",
]);
const blockedRequestHeaders = new Set([
  "connection",
  "content-length",
  "host",
  "keep-alive",
  "proxy-authenticate",
  "proxy-authorization",
  "te",
  "trailer",
  "transfer-encoding",
  "upgrade",
]);
const blockedResponseHeaders = new Set([
  ...blockedRequestHeaders,
  "content-encoding",
  "content-length",
  "set-cookie",
]);
const forwardedRequestHeaders = new Set([
  "accept",
  "accept-language",
  "content-type",
  "cookie",
  "x-csrf-token",
  "x-requested-with",
]);

function buildBackendUrl(pathSegments: string[]): string | null {
  if (pathSegments.length !== 1) {
    return null;
  }

  const [path] = pathSegments;
  if (!allowedAuthPaths.has(path)) {
    return null;
  }

  return new URL(`/auth/${path}`, serverApiEnv.baseUrl).toString();
}

function buildRequestHeaders(request: NextRequest): Headers {
  const headers = new Headers();

  request.headers.forEach((value, key) => {
    const normalizedKey = key.toLowerCase();

    if (
      blockedRequestHeaders.has(normalizedKey) ||
      !forwardedRequestHeaders.has(normalizedKey)
    ) {
      return;
    }

    headers.set(key, value);
  });

  appendForwardedHeadersFromRequest(headers, request);
  return headers;
}

function buildResponseHeaders(backendResponse: Response): Headers {
  const headers = new Headers();

  backendResponse.headers.forEach((value, key) => {
    if (blockedResponseHeaders.has(key.toLowerCase())) {
      return;
    }

    headers.set(key, value);
  });

  headers.set("Cache-Control", "no-store");
  return headers;
}

function createProblemResponse(status: number, title: string): NextResponse {
  return NextResponse.json(
    {
      title,
      status,
    },
    {
      status,
      headers: {
        "Cache-Control": "no-store",
      },
    },
  );
}

async function proxyAuthRequest(
  request: NextRequest,
  context: RouteContext,
): Promise<NextResponse> {
  const { path } = await context.params;
  const backendUrl = buildBackendUrl(path);

  if (!backendUrl) {
    return createProblemResponse(404, "Auth route not found.");
  }

  const method = request.method.toUpperCase();
  const body = noBodyMethods.has(method) ? undefined : await request.arrayBuffer();

  let backendResponse: Response;
  try {
    backendResponse = await fetch(backendUrl, {
      method,
      headers: buildRequestHeaders(request),
      body,
      cache: "no-store",
    });
  } catch {
    return createProblemResponse(502, "Backend unavailable.");
  }

  const responseBody =
    method === "HEAD" || noResponseBodyStatuses.has(backendResponse.status)
      ? null
      : await backendResponse.arrayBuffer();

  const response = new NextResponse(responseBody, {
    status: backendResponse.status,
    statusText: backendResponse.statusText,
    headers: buildResponseHeaders(backendResponse),
  });

  appendAuthSetCookieHeaders(response, getSetCookieHeaders(backendResponse));
  return response;
}

export function GET(request: NextRequest, context: RouteContext) {
  return proxyAuthRequest(request, context);
}

export function POST(request: NextRequest, context: RouteContext) {
  return proxyAuthRequest(request, context);
}
