import { cookies } from "next/headers";

import type { Exercise, ExerciseListSource } from "@/entities/exercise/model/types";
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

export async function getExercises(source: ExerciseListSource = "all"): Promise<Exercise[]> {
  const cookieStore = await cookies();
  const cookieHeader = buildCookieHeader(cookieStore.getAll());
  const url = new URL("/api/exercises", apiEnv.baseUrl);
  url.searchParams.set("source", source);

  const response = await fetch(url, {
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

  return (await response.json()) as Exercise[];
}
