import "server-only";

import type { NextRequest } from "next/server";

const forwardedForHeader = "X-Forwarded-For";
const forwardedHostHeader = "X-Forwarded-Host";
const forwardedProtoHeader = "X-Forwarded-Proto";

type HeaderSource = Pick<Headers, "get">;

function normalizeProtocol(protocol: string | null | undefined): string | null {
  const normalized = protocol?.trim().replace(/:$/, "").toLowerCase();
  return normalized || null;
}

function resolveForwardedProto(sourceHeaders: HeaderSource, protocol?: string): string {
  const forwardedProto = normalizeProtocol(sourceHeaders.get(forwardedProtoHeader));
  if (forwardedProto) {
    return forwardedProto;
  }

  if (process.env.NODE_ENV === "production") {
    return "https";
  }

  return normalizeProtocol(protocol) ?? "http";
}

function resolveForwardedHost(sourceHeaders: HeaderSource): string | null {
  return sourceHeaders.get(forwardedHostHeader) ?? sourceHeaders.get("host");
}

export function appendForwardedHeaders(
  targetHeaders: Headers,
  sourceHeaders: HeaderSource,
  protocol?: string,
) {
  targetHeaders.set(forwardedProtoHeader, resolveForwardedProto(sourceHeaders, protocol));

  const forwardedHost = resolveForwardedHost(sourceHeaders);
  if (forwardedHost) {
    targetHeaders.set(forwardedHostHeader, forwardedHost);
  }

  const forwardedFor = sourceHeaders.get(forwardedForHeader);
  if (forwardedFor) {
    targetHeaders.set(forwardedForHeader, forwardedFor);
  }
}

export function appendForwardedHeadersFromRequest(
  targetHeaders: Headers,
  request: NextRequest,
) {
  appendForwardedHeaders(targetHeaders, request.headers, request.nextUrl.protocol);
}
