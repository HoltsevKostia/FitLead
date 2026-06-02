import { ApiError } from "@/lib/api/api-error";
import { CSRF_HEADER_NAME, ensureCsrfToken, isUnsafeMethod } from "@/lib/api/csrf";

type ResponseType = "json" | "text" | "void";

export interface ApiRequestOptions extends Omit<RequestInit, "body" | "credentials"> {
  body?: BodyInit | object | null;
  credentials?: RequestCredentials;
  responseType?: ResponseType;
}

function buildApiUrl(path: string): string {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;

  if (normalizedPath.startsWith("/auth/")) {
    return `/api${normalizedPath}`;
  }

  if (normalizedPath === "/api" || normalizedPath.startsWith("/api/")) {
    throw new Error(
      "Browser apiRequest business paths must not include the /api prefix. Use /exercises instead of /api/exercises.",
    );
  }

  return `/api/backend${normalizedPath}`;
}

function isBodyInit(value: unknown): value is BodyInit {
  return (
    typeof value === "string" ||
    value instanceof Blob ||
    value instanceof FormData ||
    value instanceof URLSearchParams ||
    value instanceof ArrayBuffer ||
    ArrayBuffer.isView(value)
  );
}

function serializeBody(body: ApiRequestOptions["body"], headers: Headers): BodyInit | undefined {
  if (body === undefined || body === null) {
    return undefined;
  }

  if (isBodyInit(body)) {
    return body;
  }

  if (!headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  return JSON.stringify(body);
}

async function readResponse<TResponse>(
  response: Response,
  responseType: ResponseType,
): Promise<TResponse> {
  if (responseType === "void" || response.status === 204) {
    return undefined as TResponse;
  }

  if (responseType === "text") {
    return (await response.text()) as TResponse;
  }

  const text = await response.text();
  if (!text) {
    return undefined as TResponse;
  }

  return JSON.parse(text) as TResponse;
}

export async function apiRequest<TResponse>(
  path: string,
  options: ApiRequestOptions = {},
): Promise<TResponse> {
  const {
    body,
    headers: initialHeaders,
    method = body === undefined ? "GET" : "POST",
    credentials = "include",
    responseType = "json",
    cache = "no-store",
    ...requestInit
  } = options;

  const headers = new Headers(initialHeaders);
  if (!headers.has("Accept") && responseType === "json") {
    headers.set("Accept", "application/json, application/problem+json");
  }

  if (credentials !== "omit" && isUnsafeMethod(method) && !headers.has(CSRF_HEADER_NAME)) {
    const csrfToken = await ensureCsrfToken();
    headers.set(CSRF_HEADER_NAME, csrfToken);
  }

  const response = await fetch(buildApiUrl(path), {
    ...requestInit,
    method,
    headers,
    body: serializeBody(body, headers),
    credentials,
    cache,
  });

  if (!response.ok) {
    throw await ApiError.fromResponse(response);
  }

  return readResponse<TResponse>(response, responseType);
}
