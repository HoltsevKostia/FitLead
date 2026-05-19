"use client";

import { useRouter } from "next/navigation";
import { type SubmitEvent, useState } from "react";

import { mapSubmitVideoReportFeedbackError } from "@/features/video-reports/model/error-mapping";
import { chatsApi } from "@/lib/api/clients/chats-api";
import { FormAlert } from "@/shared/forms/form-alert";
import { fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";

interface SubmitVideoReportFeedbackFormProps {
  chatId: string;
  reportId: string;
}

export function SubmitVideoReportFeedbackForm({
  chatId,
  reportId,
}: SubmitVideoReportFeedbackFormProps) {
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    const formData = new FormData(event.currentTarget);
    const text = String(formData.get("text") ?? "").trim();

    if (!text) {
      setError("Вкажіть відгук.");
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      await chatsApi.submitVideoReportFeedback(chatId, reportId, { text });
      router.refresh();
    } catch (caughtError) {
      setError(mapSubmitVideoReportFeedbackError(caughtError));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-4 rounded-2xl border border-border bg-white px-5 py-5"
    >
      <div>
        <h2 className="text-base font-semibold text-foreground">Відгук тренера</h2>
        <p className="mt-1 text-sm text-muted">
          Додайте коментар щодо техніки виконання.
        </p>
      </div>

      <FormAlert message={error} />

      <div className="space-y-2">
        <label htmlFor="trainer-feedback-text" className={fieldLabelClassName}>
          Відгук
        </label>
        <textarea
          id="trainer-feedback-text"
          name="text"
          required
          maxLength={4000}
          rows={5}
          disabled={isSubmitting}
          className={`${fieldInputClassName} resize-y`}
          placeholder="Наприклад, контролюй коліна у нижній фазі."
        />
      </div>

      <div className="flex justify-end">
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-full bg-accent px-5 py-3 text-sm font-semibold text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-60"
        >
          {isSubmitting ? "Надсилаємо..." : "Надіслати відгук"}
        </button>
      </div>
    </form>
  );
}
