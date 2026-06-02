import "server-only";

function normalizeBaseUrl(value: string): string {
  return value.endsWith("/") ? value.slice(0, -1) : value;
}

function resolveServerApiBaseUrl(): string {
  const configuredBaseUrl = process.env.API_BASE_URL?.trim();

  if (configuredBaseUrl) {
    return normalizeBaseUrl(configuredBaseUrl);
  }

  if (process.env.NODE_ENV !== "production") {
    return "http://localhost:5178";
  }

  throw new Error("Environment variable API_BASE_URL is required for server API calls.");
}

export const serverApiEnv = {
  get baseUrl() {
    return resolveServerApiBaseUrl();
  },
} as const;
