import { isApiError } from "@/lib/api/api-error";

export function mapWorkoutMutationError(error: unknown): string {
  if (!isApiError(error)) {
    return "Не вдалося виконати дію. Спробуйте ще раз.";
  }

  if (error.errorCode === "workout.create.name_required") {
    return "Вкажіть назву тренування.";
  }

  return error.detail ?? error.title ?? "Не вдалося виконати дію. Спробуйте ще раз.";
}
