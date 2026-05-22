/* eslint-disable @next/next/no-img-element */

import type { MediaAssetPreview } from "@/entities/media-asset/model/types";

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
  if (!mediaAsset) {
    return null;
  }

  if (mediaAsset.kind === "Image") {
    return (
      <a
        href={mediaAsset.deliveryUrl}
        target="_blank"
        rel="noreferrer"
        className="block w-fit overflow-hidden rounded-lg border border-border bg-surface transition hover:border-accent"
        aria-label="Відкрити медіа вправи"
      >
        <img
          src={mediaAsset.deliveryUrl}
          alt=""
          className="h-16 w-24 object-cover"
          loading="lazy"
        />
      </a>
    );
  }

  const label = mediaAsset.kind === "Video" ? "Відео" : "Медіа";

  return (
    <a href={mediaAsset.deliveryUrl} target="_blank" rel="noreferrer" className="w-fit">
      <MediaBadge label={label} />
    </a>
  );
}
