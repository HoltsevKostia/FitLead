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

export function mapCreateInvitationError(error: unknown): string {
  if (!isApiError(error)) {
    return "Не вдалося створити запрошення. Спробуй ще раз.";
  }

  if (error.status === 400) {
    return error.detail ?? "Вибрано невалідний строк дії запрошення.";
  }

  if (error.status === 403) {
    return "Створювати запрошення може тільки тренер.";
  }

  if (error.status >= 500) {
    return "На сервері сталася помилка. Спробуй пізніше.";
  }

  return error.detail ?? "Не вдалося створити запрошення. Спробуй ще раз.";
}

export function mapRevokeInvitationError(error: unknown): string {
  if (!isApiError(error)) {
    return "Не вдалося відкликати запрошення. Спробуй ще раз.";
  }

  if (error.status === 404) {
    return "Запрошення вже не знайдено.";
  }

  if (error.status === 409) {
    return error.detail ?? "Запрошення вже недоступне або його стан змінився.";
  }

  if (error.status === 403) {
    return "Відкликати запрошення може тільки тренер.";
  }

  if (error.status >= 500) {
    return "На сервері сталася помилка. Спробуй пізніше.";
  }

  return error.detail ?? "Не вдалося відкликати запрошення. Спробуй ще раз.";
}
