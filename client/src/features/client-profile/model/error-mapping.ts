import { isApiError } from "@/lib/api/api-error";

export function mapClientProfileMutationError(error: unknown): string {
  if (!isApiError(error)) {
    return "Не вдалося зберегти профіль. Спробуйте ще раз.";
  }

  if (error.status === 401) {
    return "Сесія завершилася. Увійдіть знову та повторіть дію.";
  }

  if (error.status === 403) {
    return "Цей розділ доступний лише клієнту.";
  }

  if (error.errorCode === "client_profile.experience_level_invalid") {
    return "Оберіть коректний рівень підготовки.";
  }

  if (
    error.errorCode === "client_profile.create.height_out_of_range" ||
    error.errorCode === "client_profile.update.height_out_of_range"
  ) {
    return "Зріст має бути від 50 до 300 см.";
  }

  if (
    error.errorCode === "client_profile.create.goal_too_long" ||
    error.errorCode === "client_profile.update.goal_too_long"
  ) {
    return "Ціль має бути не довшою за 500 символів.";
  }

  if (
    error.errorCode === "client_profile.create.limitations_too_long" ||
    error.errorCode === "client_profile.update.limitations_too_long" ||
    error.errorCode === "client_profile.create.training_preferences_too_long" ||
    error.errorCode === "client_profile.update.training_preferences_too_long" ||
    error.errorCode === "client_profile.create.additional_info_too_long" ||
    error.errorCode === "client_profile.update.additional_info_too_long"
  ) {
    return "Текстове поле має бути не довшим за 1000 символів.";
  }

  if (error.status >= 500) {
    return "На сервері сталася помилка. Спробуйте пізніше.";
  }

  return error.detail ?? error.title ?? "Не вдалося зберегти профіль. Спробуйте ще раз.";
}
