import type { BodyMetricEntry } from "@/entities/body-metric/model/types";
import { PlainText } from "@/shared/ui/plain-text";

import { formatBodyMetricDate, getMetricSummary } from "./body-metric-form-utils";

interface BodyMetricEntryCardProps {
  entry: BodyMetricEntry;
  isDeleting: boolean;
  onEdit: (entry: BodyMetricEntry) => void;
  onDelete: (entry: BodyMetricEntry) => void;
}

export function BodyMetricEntryCard({
  entry,
  isDeleting,
  onEdit,
  onDelete,
}: BodyMetricEntryCardProps) {
  const summary = getMetricSummary(entry);

  return (
    <article className="rounded-2xl border border-border bg-white p-4 shadow-sm">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0">
          <h3 className="text-base font-semibold text-foreground">
            {formatBodyMetricDate(entry.recordedAt)}
          </h3>
          {summary.length > 0 ? (
            <div className="mt-3 flex flex-wrap gap-2">
              {summary.map((item) => (
                <span
                  key={item}
                  className="rounded-full bg-surface-strong px-3 py-1 text-xs font-medium text-foreground"
                >
                  {item}
                </span>
              ))}
            </div>
          ) : null}
        </div>

        <div className="flex shrink-0 flex-wrap gap-2">
          <button
            type="button"
            onClick={() => onEdit(entry)}
            className="inline-flex min-h-9 items-center justify-center rounded-lg border border-border px-3 py-1.5 text-sm font-medium text-foreground transition hover:bg-surface-strong"
          >
            Редагувати
          </button>
          <button
            type="button"
            onClick={() => onDelete(entry)}
            disabled={isDeleting}
            className="inline-flex min-h-9 items-center justify-center rounded-lg border border-red-200 px-3 py-1.5 text-sm font-medium text-red-700 transition hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-70"
          >
            {isDeleting ? "Видаляємо..." : "Видалити"}
          </button>
        </div>
      </div>

      {entry.note ? (
        <PlainText className="mt-4 text-sm leading-6 text-muted">{entry.note}</PlainText>
      ) : null}
    </article>
  );
}
