import { cookies } from "next/headers";

import type { CurrentUser } from "@/features/auth/model/types";
import { apiEnv } from "@/lib/api/env";

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

export async function getCurrentUser(): Promise<CurrentUser | null> {
  const cookieStore = await cookies();
  const cookieHeader = buildCookieHeader(cookieStore.getAll());

  if (!cookieHeader) {
    return null;
  }

  const response = await fetch(new URL("/auth/current-user", apiEnv.baseUrl), {
    method: "GET",
    headers: {
      Accept: "application/json, application/problem+json",
      Cookie: cookieHeader,
    },
    cache: "no-store",
  });

  if (response.status === 401) {
    return null;
  }

  if (!response.ok) {
    throw new Error(`Failed to resolve current user. Status: ${response.status}`);
  }

  return (await response.json()) as CurrentUser;
}
