import { isApiError } from "@/lib/api/api-error";

export function mapProgressPhotoMutationError(error: unknown): string {
  if (!isApiError(error)) {
    return "Не вдалося зберегти фото. Спробуйте ще раз.";
  }

  if (error.status === 401) {
    return "Сесія завершилася. Увійдіть знову та повторіть дію.";
  }

  if (error.status === 403) {
    return "Цей розділ доступний лише клієнту.";
  }

  if (error.status === 404) {
    return "Фото або медіафайл не знайдено.";
  }

  if (error.errorCode === "client_progress_photo.create.label_invalid") {
    return "Оберіть коректний тип фото.";
  }

  if (error.errorCode === "client_progress_photo.create.taken_at_required") {
    return "Оберіть дату фото.";
  }

  if (error.errorCode === "media_asset.kind_not_allowed_for_progress_photo") {
    return "Для фото прогресу можна додати тільки зображення.";
  }

  if (error.errorCode === "media_asset.inactive") {
    return "Медіафайл недоступний. Оберіть інше фото.";
  }

  if (error.errorCode === "client_progress_photo.create.note_too_long") {
    return "Нотатка має бути не довшою за 1000 символів.";
  }

  if (error.status >= 500) {
    return "На сервері сталася помилка. Спробуйте пізніше.";
  }

  return error.detail ?? error.title ?? "Не вдалося зберегти фото. Спробуйте ще раз.";
}
