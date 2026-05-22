/* eslint-disable @next/next/no-img-element */

import type { MediaAssetPreview } from "@/entities/media-asset/model/types";

interface ExerciseDetailMediaProps {
  mediaAsset: MediaAssetPreview | null;
}

export function ExerciseDetailMedia({ mediaAsset }: ExerciseDetailMediaProps) {
  if (!mediaAsset) {
    return (
      <div className="rounded-2xl border border-dashed border-border px-5 py-6 text-sm text-muted">
        Медіа не додано.
      </div>
    );
  }

  if (mediaAsset.kind === "Image") {
    return (
      <a
        href={mediaAsset.deliveryUrl}
        target="_blank"
        rel="noreferrer"
        className="block overflow-hidden rounded-2xl border border-border bg-surface"
      >
        <img
          src={mediaAsset.deliveryUrl}
          alt=""
          className="max-h-[520px] w-full object-contain"
        />
      </a>
    );
  }

  if (mediaAsset.kind === "Video") {
    return (
      <video
        controls
        className="max-h-[520px] w-full rounded-2xl border border-border bg-black"
        src={mediaAsset.deliveryUrl}
      >
        <a href={mediaAsset.deliveryUrl} target="_blank" rel="noreferrer">
          Відкрити медіа
        </a>
      </video>
    );
  }

  return (
    <div className="rounded-2xl border border-border bg-surface px-5 py-6">
      <p className="text-sm text-muted">Медіа недоступне для перегляду.</p>
    </div>
  );
}
