import { isApiError } from "@/lib/api/api-error";

export function mapExerciseMutationError(error: unknown): string {
  if (!isApiError(error)) {
    return "Не вдалося виконати дію. Спробуй ще раз.";
  }

  if (error.errorCode === "exercise.not_found") {
    return "Вправу не знайдено або у вас немає доступу до неї.";
  }

  if (error.errorCode === "exercise.in_use") {
    return "Вправа використовується у тренуваннях. Видалення з підтвердженням буде додано окремо.";
  }

  if (error.status === 400) {
    return "Перевірте дані вправи і спробуйте ще раз.";
  }

  return error.detail ?? "Не вдалося виконати дію. Спробуй ще раз.";
}
