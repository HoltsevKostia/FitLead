import type { CurrentUser } from "@/features/auth/model/types";
import { isUnauthorizedApiError } from "@/lib/api/api-error";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getCurrentUser(): Promise<CurrentUser | null> {
  try {
    return await serverApiRequest<CurrentUser>("/auth/current-user");
  } catch (error) {
    if (isUnauthorizedApiError(error)) {
      return null;
    }

    throw error;
  }
}
