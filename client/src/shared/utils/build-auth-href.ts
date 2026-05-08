export function buildAuthHref(path: "/login" | "/register", nextHref: string): string {
  if (nextHref === "/dashboard") {
    return path;
  }

  return `${path}?next=${encodeURIComponent(nextHref)}`;
}
