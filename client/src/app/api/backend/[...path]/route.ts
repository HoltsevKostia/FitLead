import { NextResponse, type NextRequest } from "next/server";

import { apiEnv } from "@/lib/api/env";

export const runtime = "nodejs";

type RouteContext = {
  params: Promise<{
    path: string[];
  }>;
};

const backendApiPrefix = "/api";
const noBodyMethods = new Set(["GET", "HEAD"]);
const noResponseBodyStatuses = new Set([204, 304]);
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

function isValidPathSegment(segment: string): boolean {
  return Boolean(segment) && segment !== "." && segment !== "..";
}

function buildBackendUrl(pathSegments: string[], search: string): string | null {
  if (pathSegments.length === 0 || pathSegments.some((segment) => !isValidPathSegment(segment))) {
    return null;
  }

  const backendPath = pathSegments
    .map((segment) => encodeURIComponent(segment))
    .join("/");

  return new URL(`${backendApiPrefix}/${backendPath}${search}`, apiEnv.baseUrl).toString();
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

async function proxyBackendRequest(
  request: NextRequest,
  context: RouteContext,
): Promise<NextResponse> {
  const { path } = await context.params;
  const backendUrl = buildBackendUrl(path, request.nextUrl.search);

  if (!backendUrl) {
    return createProblemResponse(400, "Invalid backend API path.");
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

  appendSetCookieHeaders(response, getSetCookieHeaders(backendResponse));
  return response;
}

export function GET(request: NextRequest, context: RouteContext) {
  return proxyBackendRequest(request, context);
}

export function HEAD(request: NextRequest, context: RouteContext) {
  return proxyBackendRequest(request, context);
}

export function POST(request: NextRequest, context: RouteContext) {
  return proxyBackendRequest(request, context);
}

export function PUT(request: NextRequest, context: RouteContext) {
  return proxyBackendRequest(request, context);
}

export function PATCH(request: NextRequest, context: RouteContext) {
  return proxyBackendRequest(request, context);
}

export function DELETE(request: NextRequest, context: RouteContext) {
  return proxyBackendRequest(request, context);
}
