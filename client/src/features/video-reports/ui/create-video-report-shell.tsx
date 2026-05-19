import Link from "next/link";

import type { ChatDetails } from "@/entities/chat/model/types";
import { fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";

interface CreateVideoReportShellProps {
  chat: ChatDetails;
}

export function CreateVideoReportShell({ chat }: CreateVideoReportShellProps) {
  return (
    <section className="mx-auto flex w-full max-w-3xl flex-col gap-6">
      <header className="space-y-3">
        <Link
          href={`/chats/${chat.id}`}
          className="text-sm font-medium text-accent hover:text-accent-strong"
        >
          Назад до чату
        </Link>
        <div>
          <p className="text-sm text-muted">Звіт для {chat.trainerName}</p>
          <h1 className="mt-1 text-2xl font-semibold text-foreground">
            Створити відео-звіт
          </h1>
        </div>
      </header>

      <form className="space-y-5 rounded-2xl border border-border bg-white px-5 py-5">
        <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-4 text-sm leading-6 text-amber-950">
          Зберігаються останні 5 активних відеозвітів у цьому чаті. Старі звіти
          можуть бути архівовані.
        </div>

        <div className="space-y-2">
          <label htmlFor="video-report-title" className={fieldLabelClassName}>
            Назва
          </label>
          <input
            id="video-report-title"
            name="title"
            required
            maxLength={200}
            className={fieldInputClassName}
            placeholder="Наприклад, присідання"
          />
        </div>

        <div className="space-y-2">
          <label
            htmlFor="video-report-description"
            className={fieldLabelClassName}
          >
            Опис
          </label>
          <textarea
            id="video-report-description"
            name="description"
            maxLength={2000}
            rows={5}
            className={`${fieldInputClassName} resize-y`}
            placeholder="Що саме потрібно перевірити"
          />
        </div>

        <div className="space-y-2">
          <p className={fieldLabelClassName}>Медіа</p>
          <div className="rounded-2xl border border-dashed border-border bg-surface px-5 py-8 text-center">
            <p className="text-sm font-medium text-foreground">Фото або відео</p>
            <p className="mt-2 text-sm text-muted">
              Додайте до 5 файлів для перевірки техніки.
            </p>
          </div>
        </div>

        <div className="flex justify-end">
          <button
            type="submit"
            disabled
            className="rounded-full bg-accent px-5 py-3 text-sm font-semibold text-white opacity-60 disabled:cursor-not-allowed"
          >
            Відправити звіт
          </button>
        </div>
      </form>
    </section>
  );
}
