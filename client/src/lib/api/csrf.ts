import { apiEnv } from "@/lib/api/env";

export const CSRF_COOKIE_NAME = "FitLead.XSRF-TOKEN";
export const CSRF_HEADER_NAME = "X-CSRF-TOKEN";

const safeMethods = new Set(["GET", "HEAD", "OPTIONS", "TRACE"]);

function buildApiUrl(path: string): string {
  return new URL(path, apiEnv.baseUrl).toString();
}

export function readCookie(name: string): string | null {
  if (typeof document === "undefined") {
    return null;
  }

  const prefix = `${name}=`;
  const cookie = document.cookie
    .split(";")
    .map((value) => value.trim())
    .find((value) => value.startsWith(prefix));

  if (!cookie) {
    return null;
  }

  return decodeURIComponent(cookie.slice(prefix.length));
}

export function isUnsafeMethod(method: string): boolean {
  return !safeMethods.has(method.toUpperCase());
}

export async function ensureCsrfToken(): Promise<string> {
  const existingToken = readCookie(CSRF_COOKIE_NAME);
  if (existingToken) {
    return existingToken;
  }

  await fetch(buildApiUrl("/auth/csrf-token"), {
    method: "GET",
    credentials: "include",
    cache: "no-store",
  });

  const issuedToken = readCookie(CSRF_COOKIE_NAME);
  if (!issuedToken) {
    throw new Error("CSRF token cookie was not issued or is not readable by the frontend.");
  }

  return issuedToken;
}

export async function refreshCsrfToken(): Promise<string> {
  await fetch(buildApiUrl("/auth/csrf-token"), {
    method: "GET",
    credentials: "include",
    cache: "no-store",
  });

  const issuedToken = readCookie(CSRF_COOKIE_NAME);
  if (!issuedToken) {
    throw new Error("CSRF token cookie was not issued or is not readable by the frontend.");
  }

  return issuedToken;
}
