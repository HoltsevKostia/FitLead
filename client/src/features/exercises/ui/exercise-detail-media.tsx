/* eslint-disable @next/next/no-img-element */

"use client";

import { useState } from "react";

import type { MediaAssetPreview } from "@/entities/media-asset/model/types";
import { MediaLightbox } from "@/features/media-assets/ui/media-lightbox";
import { MediaVideo } from "@/features/media-assets/ui/media-video";

interface ExerciseDetailMediaProps {
  mediaAsset: MediaAssetPreview | null;
}

export function ExerciseDetailMedia({ mediaAsset }: ExerciseDetailMediaProps) {
  const [isViewerOpen, setIsViewerOpen] = useState(false);

  if (!mediaAsset) {
    return (
      <div className="rounded-2xl border border-dashed border-border px-5 py-6 text-sm text-muted">
        Медіа не додано.
      </div>
    );
  }

  if (mediaAsset.kind === "Image") {
    return (
      <>
        <button
          type="button"
          onClick={() => setIsViewerOpen(true)}
          className="block w-full overflow-hidden rounded-2xl border border-border bg-surface"
          aria-label="Відкрити медіа вправи"
        >
          <img
            src={mediaAsset.deliveryUrl}
            alt=""
            className="max-h-[520px] w-full object-contain"
          />
        </button>

        {isViewerOpen ? (
          <MediaLightbox
            asset={mediaAsset}
            title={mediaAsset.fileName ?? "Фото вправи"}
            onClose={() => setIsViewerOpen(false)}
          />
        ) : null}
      </>
    );
  }

  if (mediaAsset.kind === "Video") {
    return (
      <div className="space-y-3">
        <MediaVideo
          className="rounded-2xl border border-border"
          objectFit="contain"
          src={mediaAsset.deliveryUrl}
        >
          <a href={mediaAsset.deliveryUrl} target="_blank" rel="noreferrer">
            Відкрити медіа
          </a>
        </MediaVideo>

        <button
          type="button"
          onClick={() => setIsViewerOpen(true)}
          className="rounded-full border border-border px-4 py-2 text-sm font-medium text-foreground transition hover:bg-surface-strong"
        >
          Відкрити у перегляді
        </button>

        {isViewerOpen ? (
          <MediaLightbox
            asset={mediaAsset}
            title={mediaAsset.fileName ?? "Відео вправи"}
            onClose={() => setIsViewerOpen(false)}
          />
        ) : null}
      </div>
    );
  }

  return (
    <div className="rounded-2xl border border-border bg-surface px-5 py-6">
      <p className="text-sm text-muted">Медіа недоступне для перегляду.</p>
    </div>
  );
}
