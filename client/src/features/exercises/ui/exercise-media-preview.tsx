/* eslint-disable @next/next/no-img-element */

import { getExerciseMediaInfo } from "@/features/exercises/model/exercise-media";

interface ExerciseMediaPreviewProps {
  mediaUrl: string | null;
}

function MediaBadge({ label }: { label: string }) {
  return (
    <span className="inline-flex items-center rounded-full border border-indigo-200 bg-indigo-50 px-3 py-1 text-xs font-medium text-indigo-800">
      {label}
    </span>
  );
}

export function ExerciseMediaPreview({ mediaUrl }: ExerciseMediaPreviewProps) {
  const media = getExerciseMediaInfo(mediaUrl);

  if (media.type === "none" || !media.url) {
    return null;
  }

  if (media.type === "image") {
    return (
      <a
        href={media.url}
        target="_blank"
        rel="noreferrer"
        className="block w-fit overflow-hidden rounded-lg border border-border bg-surface transition hover:border-accent"
        aria-label="Відкрити медіа вправи"
      >
        <img
          src={media.url}
          alt=""
          className="h-16 w-24 object-cover"
          loading="lazy"
          referrerPolicy="no-referrer"
        />
      </a>
    );
  }

  const labelByType = {
    youtube: "YouTube відео",
    video: "Відео",
    external: "Медіа",
  } satisfies Record<typeof media.type, string>;

  return (
    <a href={media.url} target="_blank" rel="noreferrer" className="w-fit">
      <MediaBadge label={labelByType[media.type]} />
    </a>
  );
}
