"use client";

function normalizeBaseUrl(value: string): string {
  return value.endsWith("/") ? value.slice(0, -1) : value;
}

function resolveSignalRBaseUrl(): string | null {
  const configuredBaseUrl = process.env.NEXT_PUBLIC_SIGNALR_BASE_URL?.trim();

  if (configuredBaseUrl) {
    return normalizeBaseUrl(configuredBaseUrl);
  }

  if (process.env.NODE_ENV === "development") {
    return "http://localhost:5178";
  }

  return null;
}

export function buildHubUrl(path: string): string {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  const baseUrl = resolveSignalRBaseUrl();

  if (!baseUrl) {
    return normalizedPath;
  }

  return new URL(normalizedPath, baseUrl).toString();
}
