"use client";

import { useState } from "react";

import type { MediaAssetPreview } from "@/entities/media-asset/model/types";
import { MediaLightbox } from "@/features/media-assets/ui/media-lightbox";
import { MediaImage } from "@/shared/ui/media-image";

interface ExerciseMediaPreviewProps {
  mediaAsset: MediaAssetPreview | null;
  exerciseName?: string;
}

function MediaBadge({ label }: { label: string }) {
  return (
    <span className="inline-flex items-center rounded-full border border-indigo-200 bg-indigo-50 px-3 py-1 text-xs font-medium text-indigo-800">
      {label}
    </span>
  );
}

export function ExerciseMediaPreview({ mediaAsset, exerciseName }: ExerciseMediaPreviewProps) {
  const [isViewerOpen, setIsViewerOpen] = useState(false);

  if (!mediaAsset) {
    return null;
  }

  const label = mediaAsset.kind === "Video" ? "Відео" : "Медіа";
  const imageAlt = exerciseName ? `Медіа вправи: ${exerciseName}` : "Медіа вправи";
  const title = mediaAsset.kind === "Image" ? imageAlt : mediaAsset.fileName ?? label;

  return (
    <>
      {mediaAsset.kind === "Image" ? (
        <button
          type="button"
          onClick={() => setIsViewerOpen(true)}
          className="block w-fit overflow-hidden rounded-lg border border-border bg-surface transition hover:border-accent"
          aria-label="Відкрити медіа вправи"
        >
          <MediaImage
            src={mediaAsset.deliveryUrl}
            alt={imageAlt}
            aspectRatio="3/2"
            className="h-16 w-24"
            sizes="96px"
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
