import type { ProgressPhoto } from "@/entities/progress-photo/model/types";
import { PlainText } from "@/shared/ui/plain-text";

interface ProgressPhotoLightboxProps {
  photo: ProgressPhoto;
  onClose: () => void;
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

export function ProgressPhotoLightbox({
  photo,
  onClose,
}: ProgressPhotoLightboxProps) {
  const formattedDate = formatDate(photo.takenAt);
  const label = getLabelText(photo.label);

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4"
      role="dialog"
      aria-modal="true"
      aria-label={`Фото прогресу: ${label}, ${formattedDate}`}
    >
      <div className="max-h-full w-full max-w-5xl overflow-hidden rounded-2xl bg-white shadow-2xl">
        <div className="flex items-start justify-between gap-4 border-b border-border px-4 py-3">
          <div className="min-w-0">
            <h2 className="text-base font-semibold text-foreground">{formattedDate}</h2>
            <p className="mt-1 text-sm text-muted">{label}</p>
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
          <img
            src={photo.mediaAsset.deliveryUrl}
            alt={`Фото прогресу: ${label}, ${formattedDate}`}
            className="mx-auto h-auto max-h-[75vh] w-auto max-w-full object-contain"
          />
        </div>

        {photo.note ? (
          <div className="border-t border-border px-4 py-3">
            <PlainText className="text-sm leading-6 text-muted">{photo.note}</PlainText>
          </div>
        ) : null}
      </div>
    </div>
  );
}
