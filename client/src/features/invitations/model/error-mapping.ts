import { isApiError } from "@/lib/api/api-error";

export function mapAcceptInvitationError(error: unknown): string {
  if (!isApiError(error)) {
    return "Не вдалося прийняти запрошення. Спробуй ще раз.";
  }

  if (error.status === 401) {
    return "Щоб приєднатися, потрібно увійти в акаунт клієнта.";
  }

  if (error.status === 403) {
    return "Прийняти запрошення може тільки клієнт.";
  }

  if (error.status === 404) {
    return "Запрошення більше не знайдено.";
  }

  if (error.status === 409) {
    return error.detail ?? "Запрошення вже недійсне або не може бути прийняте.";
  }

  if (error.status >= 500) {
    return "На сервері сталася помилка. Спробуй пізніше.";
  }

  return error.detail ?? "Не вдалося прийняти запрошення. Спробуй ще раз.";
}
