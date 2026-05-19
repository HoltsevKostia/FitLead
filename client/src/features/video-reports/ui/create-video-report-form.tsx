"use client";

import { useRouter } from "next/navigation";
import { type SubmitEvent, useRef, useState } from "react";

import type { MediaAsset } from "@/entities/media-asset/model/types";
import type { ChatDetails } from "@/entities/chat/model/types";
import { mapCreateVideoReportError } from "@/features/video-reports/model/error-mapping";
import {
  type VideoReportMediaUploaderRef,
  VideoReportMediaUploader,
} from "@/features/video-reports/ui/video-report-media-uploader";
import { chatsApi } from "@/lib/api/clients/chats-api";
import { mediaAssetsApi } from "@/lib/api/clients/media-assets-api";
import { FormAlert } from "@/shared/forms/form-alert";
import { fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";

interface CreateVideoReportFormProps {
  chat: ChatDetails;
}

async function registerMediaAssets(media: Awaited<ReturnType<VideoReportMediaUploaderRef["uploadSelectedFiles"]>>): Promise<MediaAsset[]> {
  const registeredAssets: MediaAsset[] = [];

  for (const item of media) {
    registeredAssets.push(await mediaAssetsApi.register(item));
  }

  return registeredAssets;
}

export function CreateVideoReportForm({ chat }: CreateVideoReportFormProps) {
  const router = useRouter();
  const uploaderRef = useRef<VideoReportMediaUploaderRef>(null);
  const [fileCount, setFileCount] = useState(0);
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    const formData = new FormData(event.currentTarget);
    const title = String(formData.get("title") ?? "").trim();
    const descriptionValue = String(formData.get("description") ?? "").trim();
    const description = descriptionValue.length > 0 ? descriptionValue : null;

    if (!title) {
      setFormError("Вкажіть назву звіту.");
      return;
    }

    if (fileCount === 0) {
      setFormError("Додайте фото або відео.");
      return;
    }

    setIsSubmitting(true);
    setFormError(null);

    try {
      const uploadedMedia = await uploaderRef.current?.uploadSelectedFiles();
      if (!uploadedMedia || uploadedMedia.length === 0) {
        throw new Error("Додайте фото або відео.");
      }

      const mediaAssets = await registerMediaAssets(uploadedMedia);

      await chatsApi.createVideoReport(chat.id, {
        title,
        description,
        mediaAssetIds: mediaAssets.map((asset) => asset.id),
      });

      uploaderRef.current?.clear();
      router.push(`/chats/${chat.id}`);
      router.refresh();
    } catch (error) {
      setFormError(mapCreateVideoReportError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-5 rounded-2xl border border-border bg-white px-5 py-5"
    >
      <FormAlert message={formError} />

      <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-4 text-sm leading-6 text-amber-950">
        Зберігаються останні 5 активних відеозвітів у цьому чаті. Старі звіти
        будуть архівовані.
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
          disabled={isSubmitting}
          className={fieldInputClassName}
          placeholder="Наприклад, присідання"
        />
      </div>

      <div className="space-y-2">
        <label htmlFor="video-report-description" className={fieldLabelClassName}>
          Опис
        </label>
        <textarea
          id="video-report-description"
          name="description"
          maxLength={2000}
          rows={5}
          disabled={isSubmitting}
          className={`${fieldInputClassName} resize-y`}
          placeholder="Що саме потрібно перевірити"
        />
      </div>

      <div className="space-y-2">
        <p className={fieldLabelClassName}>Медіа</p>
        <VideoReportMediaUploader
          ref={uploaderRef}
          onFileCountChange={setFileCount}
          onUploadFailed={setFormError}
        />
      </div>

      <div className="flex justify-end">
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-full bg-accent px-5 py-3 text-sm font-semibold text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-60"
        >
          {isSubmitting ? "Відправляємо..." : "Відправити звіт"}
        </button>
      </div>
    </form>
  );
}
