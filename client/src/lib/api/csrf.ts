export const CSRF_COOKIE_NAME = "FitLead.XSRF-TOKEN";
export const CSRF_HEADER_NAME = "X-CSRF-TOKEN";

const safeMethods = new Set(["GET", "HEAD", "OPTIONS", "TRACE"]);

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

async function issueCsrfToken(): Promise<string> {
  const response = await fetch("/api/auth/csrf-token", {
    method: "GET",
    credentials: "include",
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error("CSRF token could not be issued.");
  }

  const issuedToken = readCookie(CSRF_COOKIE_NAME);
  if (!issuedToken) {
    throw new Error("CSRF token cookie was not issued or is not readable by the frontend.");
  }

  return issuedToken;
}

export function readCsrfToken(): string | null {
  return readCookie(CSRF_COOKIE_NAME);
}

export function ensureCsrfToken(): Promise<string> {
  return issueCsrfToken();
}

export async function refreshCsrfToken(): Promise<string> {
  return issueCsrfToken();
}
