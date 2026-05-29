/* eslint-disable @next/next/no-img-element */

"use client";

import { useState } from "react";

import type { MediaAssetPreview } from "@/entities/media-asset/model/types";
import { MediaLightbox } from "@/features/media-assets/ui/media-lightbox";

interface ExerciseMediaPreviewProps {
  mediaAsset: MediaAssetPreview | null;
}

function MediaBadge({ label }: { label: string }) {
  return (
    <span className="inline-flex items-center rounded-full border border-indigo-200 bg-indigo-50 px-3 py-1 text-xs font-medium text-indigo-800">
      {label}
    </span>
  );
}

export function ExerciseMediaPreview({ mediaAsset }: ExerciseMediaPreviewProps) {
  const [isViewerOpen, setIsViewerOpen] = useState(false);

  if (!mediaAsset) {
    return null;
  }

  const label = mediaAsset.kind === "Video" ? "Відео" : "Медіа";
  const title =
    mediaAsset.fileName ?? (mediaAsset.kind === "Image" ? "Фото вправи" : label);

  return (
    <>
      {mediaAsset.kind === "Image" ? (
        <button
          type="button"
          onClick={() => setIsViewerOpen(true)}
          className="block w-fit overflow-hidden rounded-lg border border-border bg-surface transition hover:border-accent"
          aria-label="Відкрити медіа вправи"
        >
          <img
            src={mediaAsset.deliveryUrl}
            alt=""
            className="h-16 w-24 object-cover"
            loading="lazy"
          />
        </button>
      ) : (
        <button
          type="button"
          onClick={() => setIsViewerOpen(true)}
          className="w-fit"
          aria-label="Відкрити медіа вправи"
        >
          <MediaBadge label={label} />
        </button>
      )}

      {isViewerOpen ? (
        <MediaLightbox
          asset={mediaAsset}
          title={title}
          onClose={() => setIsViewerOpen(false)}
        />
      ) : null}
    </>
  );
}
