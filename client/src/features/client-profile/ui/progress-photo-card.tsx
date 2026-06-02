import type { ProgressPhoto } from "@/entities/progress-photo/model/types";
import { MediaImage } from "@/shared/ui/media-image";
import { PlainText } from "@/shared/ui/plain-text";

interface ProgressPhotoCardProps {
  photo: ProgressPhoto;
  isDeleting: boolean;
  onOpen: (photo: ProgressPhoto) => void;
  onDelete: (photo: ProgressPhoto) => void;
}

const labelText: Record<ProgressPhoto["label"], string> = {
  Front: "Спереду",
  Side: "Збоку",
  Back: "Ззаду",
  Other: "Інше",
};

function getLabelText(label: ProgressPhoto["label"]): string {
  return labelText[label] ?? "Фото";
}

function formatDate(value: string): string {
  const [year, month, day] = value.split("-").map(Number);
  if (!year || !month || !day) {
    return value;
  }

  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(year, month - 1, day));
}

export function ProgressPhotoCard({
  photo,
  isDeleting,
  onOpen,
  onDelete,
}: ProgressPhotoCardProps) {
  const formattedDate = formatDate(photo.takenAt);
  const label = getLabelText(photo.label);

  return (
    <article className="overflow-hidden rounded-2xl border border-border bg-white shadow-sm">
      <button
        type="button"
        onClick={() => onOpen(photo)}
        className="block aspect-[4/5] w-full bg-surface-strong text-left"
        aria-label={`Відкрити фото прогресу: ${label}, ${formattedDate}`}
      >
        <MediaImage
          src={photo.mediaAsset.deliveryUrl}
          alt={`Фото прогресу: ${label}, ${formattedDate}`}
          aspectRatio="4/5"
          className="h-full w-full"
          sizes="(max-width: 640px) 100vw, (max-width: 1280px) 50vw, 33vw"
        />
      </button>

      <div className="space-y-3 p-4">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <h3 className="text-base font-semibold text-foreground">
              {formattedDate}
            </h3>
            <p className="mt-1 text-sm text-muted">{label}</p>
          </div>

          <button
            type="button"
            onClick={() => onDelete(photo)}
            disabled={isDeleting}
            className="inline-flex min-h-9 shrink-0 items-center justify-center rounded-lg border border-red-200 px-3 py-1.5 text-sm font-medium text-red-700 transition hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-70"
          >
            {isDeleting ? "Видаляємо..." : "Видалити"}
          </button>
        </div>

        {photo.note ? (
          <PlainText className="text-sm leading-6 text-muted">{photo.note}</PlainText>
        ) : null}
      </div>
    </article>
  );
}
