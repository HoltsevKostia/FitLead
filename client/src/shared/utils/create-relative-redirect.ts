export function createRelativeRedirect(location: string): Response {
  if (!location.startsWith("/") || location.startsWith("//")) {
    throw new Error("Relative redirect location must be an internal path.");
  }

  return new Response(null, {
    status: 307,
    headers: {
      Location: location,
    },
  });
}
