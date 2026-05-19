import { isApiError } from "@/lib/api/api-error";

export function mapCreateVideoReportError(error: unknown): string {
  if (!isApiError(error)) {
    if (error instanceof Error) {
      return error.message;
    }

    return "Не вдалося відправити звіт. Спробуйте ще раз.";
  }

  if (error.errorCode === "media_asset.kind_not_allowed_for_video_report") {
    return "Додайте лише фото або відео.";
  }

  if (error.status === 400) {
    return error.detail ?? "Перевірте дані звіту.";
  }

  if (error.status === 404) {
    return "Чат або медіа недоступні.";
  }

  if (error.status >= 500) {
    return "Не вдалося відправити звіт. Спробуйте ще раз.";
  }

  return error.detail ?? error.title ?? "Не вдалося відправити звіт. Спробуйте ще раз.";
}
