import { isApiError } from "@/lib/api/api-error";

export function mapWorkoutLogMutationError(error: unknown): string {
  if (!isApiError(error)) {
    return "Не вдалося зберегти результат тренування. Спробуй ще раз.";
  }

  if (error.status === 401) {
    return "Сесія завершилася. Увійдіть знову та повторіть дію.";
  }

  if (error.status === 403) {
    return "Тільки клієнт може виконувати цю дію.";
  }

  if (error.status === 404) {
    return "Тренування не знайдено або програма більше не доступна.";
  }

  if (
    error.errorCode === "workout_log.status_required" ||
    error.errorCode === "workout_log.create.status_invalid" ||
    error.errorCode === "workout_log.update.status_invalid"
  ) {
    return "Статус тренування некоректний.";
  }

  if (
    error.errorCode === "workout_log.create.performed_at_required" ||
    error.errorCode === "workout_log.update.performed_at_required"
  ) {
    return "Вкажіть дату виконання тренування.";
  }

  if (
    error.errorCode === "workout_log.create.skipped_performed_at_not_allowed" ||
    error.errorCode === "workout_log.update.skipped_performed_at_not_allowed" ||
    error.errorCode === "workout_log.skipped_performed_at_not_allowed"
  ) {
    return "Для пропущеного тренування дата виконання не вказується.";
  }

  if (
    error.errorCode === "workout_log.create.skipped_difficulty_rating_not_allowed" ||
    error.errorCode === "workout_log.update.skipped_difficulty_rating_not_allowed" ||
    error.errorCode === "workout_log.skipped_difficulty_rating_not_allowed"
  ) {
    return "Для пропущеного тренування оцінка складності не вказується.";
  }

  if (
    error.errorCode === "workout_log.create.difficulty_rating_out_of_range" ||
    error.errorCode === "workout_log.update.difficulty_rating_out_of_range"
  ) {
    return "Оцінка складності має бути від 1 до 10.";
  }

  if (
    error.errorCode === "workout_log.create.client_note_too_long" ||
    error.errorCode === "workout_log.update.client_note_too_long"
  ) {
    return "Коментар має бути не довшим за 1000 символів.";
  }

  if (error.status === 429) {
    return "Забагато спроб. Спробуйте пізніше.";
  }

  if (error.status >= 500) {
    return "На сервері сталася помилка. Спробуйте пізніше.";
  }

  return error.detail ?? error.title ?? "Не вдалося зберегти результат тренування. Спробуй ще раз.";
}
