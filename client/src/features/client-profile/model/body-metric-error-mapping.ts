import { isApiError } from "@/lib/api/api-error";

export function mapBodyMetricMutationError(error: unknown): string {
  if (!isApiError(error)) {
    return "Не вдалося зберегти запис. Спробуйте ще раз.";
  }

  if (error.status === 401) {
    return "Сесія завершилася. Увійдіть знову та повторіть дію.";
  }

  if (error.status === 403) {
    return "Цей розділ доступний лише клієнту.";
  }

  if (error.status === 404) {
    return "Запис не знайдено.";
  }

  if (error.errorCode === "body_metric_entry.recorded_at_conflict") {
    return "Запис на цю дату вже існує. Відредагуйте наявний запис.";
  }

  if (
    error.errorCode === "body_metric_entry.create.empty_entry" ||
    error.errorCode === "body_metric_entry.update.empty_entry"
  ) {
    return "Заповніть хоча б одну метрику або нотатку.";
  }

  if (
    error.errorCode?.includes("weight_kg_out_of_range") ||
    error.errorCode?.includes("body_fat_percent_out_of_range") ||
    error.errorCode?.includes("_cm_out_of_range")
  ) {
    return "Перевірте числові значення метрик.";
  }

  if (error.errorCode?.includes("note_too_long")) {
    return "Нотатка має бути не довшою за 1000 символів.";
  }

  if (error.status >= 500) {
    return "На сервері сталася помилка. Спробуйте пізніше.";
  }

  return error.detail ?? error.title ?? "Не вдалося зберегти запис. Спробуйте ще раз.";
}
