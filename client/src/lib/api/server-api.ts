import { cookies } from "next/headers";

import { ApiError } from "@/lib/api/api-error";
import { serverApiEnv } from "@/lib/api/server-env";

type ServerResponseType = "json" | "text" | "void";

interface ServerApiRequestOptions extends Omit<RequestInit, "credentials"> {
  responseType?: ServerResponseType;
}

interface CookieEntry {
  name: string;
  value: string;
}

function buildCookieHeader(cookieEntries: ReadonlyArray<CookieEntry>): string | null {
  if (cookieEntries.length === 0) {
    return null;
  }

  return cookieEntries.map(({ name, value }) => `${name}=${value}`).join("; ");
}

function buildApiUrl(path: string): string {
  return new URL(path, serverApiEnv.baseUrl).toString();
}

async function readResponse<TResponse>(
  response: Response,
  responseType: ServerResponseType,
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

export async function serverApiRequest<TResponse>(
  path: string,
  options: ServerApiRequestOptions = {},
): Promise<TResponse> {
  const {
    headers: initialHeaders,
    responseType = "json",
    cache = "no-store",
    ...requestInit
  } = options;

  const cookieStore = await cookies();
  const cookieHeader = buildCookieHeader(cookieStore.getAll());
  const headers = new Headers(initialHeaders);

  if (!headers.has("Accept") && responseType === "json") {
    headers.set("Accept", "application/json, application/problem+json");
  }

  if (cookieHeader && !headers.has("Cookie")) {
    headers.set("Cookie", cookieHeader);
  }

  const response = await fetch(buildApiUrl(path), {
    ...requestInit,
    headers,
    cache,
  });

  if (!response.ok) {
    throw await ApiError.fromResponse(response);
  }

  return readResponse<TResponse>(response, responseType);
}
