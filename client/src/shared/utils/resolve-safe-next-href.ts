export function resolveSafeNextHref(
  value: string | string[] | undefined,
  fallback: string,
): string {
  if (Array.isArray(value) || !value) {
    return fallback;
  }

  if (!value.startsWith("/") || value.startsWith("//")) {
    return fallback;
  }

  try {
    const url = new URL(value, "http://localhost");
    const href = `${url.pathname}${url.search}${url.hash}`;

    if (href === "/login" || href.startsWith("/login?")) {
      return fallback;
    }

    if (href === "/register" || href.startsWith("/register?")) {
      return fallback;
    }

    return href;
  } catch {
    return fallback;
  }
}
