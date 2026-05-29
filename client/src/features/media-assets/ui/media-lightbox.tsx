"use client";

import type { ReactNode } from "react";

import type { MediaAssetPreview } from "@/entities/media-asset/model/types";
import { MediaVideo } from "@/features/media-assets/ui/media-video";
import { MediaImage } from "@/shared/ui/media-image";
import { PlainText } from "@/shared/ui/plain-text";

export type MediaLightboxAsset = Pick<
  MediaAssetPreview,
  "deliveryUrl" | "kind" | "fileName" | "contentType" | "durationSeconds"
>;

interface MediaLightboxProps {
  asset: MediaLightboxAsset;
  title?: ReactNode;
  subtitle?: ReactNode;
  note?: ReactNode;
  onClose: () => void;
}

function getMediaLabel(asset: MediaLightboxAsset): string {
  if (asset.kind === "Image") {
    return "Фото";
  }

  if (asset.kind === "Video") {
    return "Відео";
  }

  return "Медіа";
}

export function MediaLightbox({
  asset,
  title,
  subtitle,
  note,
  onClose,
}: MediaLightboxProps) {
  const mediaLabel = getMediaLabel(asset);
  const accessibleTitle = typeof title === "string" ? title : asset.fileName ?? mediaLabel;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4"
      role="dialog"
      aria-modal="true"
      aria-label={accessibleTitle}
    >
      <div className="max-h-full w-full max-w-5xl overflow-hidden rounded-2xl bg-white shadow-2xl">
        <div className="flex items-start justify-between gap-4 border-b border-border px-4 py-3">
          <div className="min-w-0">
            {title ? (
              <h2 className="break-words text-base font-semibold text-foreground">
                {title}
              </h2>
            ) : null}

            {subtitle ? (
              <div className="mt-1 break-words text-sm text-muted">{subtitle}</div>
            ) : null}
          </div>

          <button
            type="button"
            onClick={onClose}
            className="inline-flex min-h-9 shrink-0 items-center justify-center rounded-lg border border-border px-3 py-1.5 text-sm font-medium text-foreground transition hover:bg-surface-strong"
          >
            Закрити
          </button>
        </div>

        <div className="max-h-[75vh] overflow-auto bg-surface-strong">
          {asset.kind === "Image" ? (
            <MediaImage
              src={asset.deliveryUrl}
              alt={accessibleTitle}
              aspectRatio="video"
              objectFit="contain"
              className="mx-auto w-full max-w-full"
              sizes="(max-width: 1024px) 100vw, 1024px"
            />
          ) : asset.kind === "Video" ? (
            <MediaVideo
              src={asset.deliveryUrl}
              className="mx-auto w-full max-w-full"
              objectFit="contain"
            >
              <a href={asset.deliveryUrl} target="_blank" rel="noreferrer">
                Відкрити відео
              </a>
            </MediaVideo>
          ) : (
            <div className="px-4 py-10 text-center text-sm text-muted">
              Цей тип медіа не підтримується для перегляду.
            </div>
          )}
        </div>

        {note ? (
          <div className="border-t border-border px-4 py-3">
            <PlainText className="text-sm leading-6 text-muted">{note}</PlainText>
          </div>
        ) : null}
      </div>
    </div>
  );
}
