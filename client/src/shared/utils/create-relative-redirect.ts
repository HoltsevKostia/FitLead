import { NextResponse } from "next/server";

export function createRelativeRedirect(location: string): NextResponse {
  if (!location.startsWith("/") || location.startsWith("//")) {
    throw new Error("Relative redirect location must be an internal path.");
  }

  return new NextResponse(null, {
    status: 307,
    headers: {
      Location: location,
    },
  });
}
