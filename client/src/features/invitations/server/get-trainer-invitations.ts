import { cookies } from "next/headers";

import type { TrainerInvitation } from "@/entities/invitation/model/types";
import { ApiError } from "@/lib/api/api-error";
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

export async function getTrainerInvitations(): Promise<TrainerInvitation[]> {
  const cookieStore = await cookies();
  const cookieHeader = buildCookieHeader(cookieStore.getAll());

  const response = await fetch(new URL("/api/invitations/trainer", apiEnv.baseUrl), {
    method: "GET",
    headers: {
      Accept: "application/json, application/problem+json",
      ...(cookieHeader ? { Cookie: cookieHeader } : {}),
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw await ApiError.fromResponse(response);
  }

  return (await response.json()) as TrainerInvitation[];
}
