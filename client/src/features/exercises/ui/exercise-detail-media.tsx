/* eslint-disable @next/next/no-img-element */

import { getExerciseMediaInfo } from "@/features/exercises/model/exercise-media";

interface ExerciseDetailMediaProps {
  mediaUrl: string | null;
}

export function ExerciseDetailMedia({ mediaUrl }: ExerciseDetailMediaProps) {
  const media = getExerciseMediaInfo(mediaUrl);

  if (media.type === "none" || !media.url) {
    return (
      <div className="rounded-2xl border border-dashed border-border px-5 py-6 text-sm text-muted">
        Медіа не додано.
      </div>
    );
  }

  if (media.type === "image") {
    return (
      <a
        href={media.url}
        target="_blank"
        rel="noreferrer"
        className="block overflow-hidden rounded-2xl border border-border bg-surface"
      >
        <img
          src={media.url}
          alt=""
          className="max-h-[520px] w-full object-contain"
          referrerPolicy="no-referrer"
        />
      </a>
    );
  }

  if (media.type === "video") {
    return (
      <video
        controls
        className="max-h-[520px] w-full rounded-2xl border border-border bg-black"
        src={media.url}
      >
        <a href={media.url} target="_blank" rel="noreferrer">
          Відкрити медіа
        </a>
      </video>
    );
  }

  return (
    <div className="rounded-2xl border border-border bg-surface px-5 py-6">
      <p className="text-sm text-muted">
        {media.type === "youtube"
          ? "YouTube відео доступне за зовнішнім посиланням."
          : "Медіа доступне за зовнішнім посиланням."}
      </p>
      <a
        href={media.url}
        target="_blank"
        rel="noreferrer"
        className="mt-4 inline-flex rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong"
      >
        Відкрити медіа
      </a>
    </div>
  );
}
