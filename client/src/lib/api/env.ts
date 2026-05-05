function normalizeBaseUrl(value: string): string {
  return value.endsWith("/") ? value.slice(0, -1) : value;
}

function resolveApiBaseUrl(): string {
  const configuredBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL?.trim();

  if (!configuredBaseUrl) {
    throw new Error(
      "Environment variable NEXT_PUBLIC_API_BASE_URL is required for the API client.",
    );
  }

  return normalizeBaseUrl(configuredBaseUrl);
}

export const apiEnv = {
  baseUrl: resolveApiBaseUrl(),
} as const;
