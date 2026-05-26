import type { FormEvent } from "react";

import type { BodyMetricEntry } from "@/entities/body-metric/model/types";
import { FormAlert } from "@/shared/forms/form-alert";
import {
  fieldErrorClassName,
  fieldInputClassName,
  fieldLabelClassName,
} from "@/shared/forms/field-styles";

import {
  type BodyMetricFieldErrors,
  type BodyMetricFormValues,
  formatBodyMetricDate,
  metricFields,
} from "./body-metric-form-utils";

interface BodyMetricFormProps {
  values: BodyMetricFormValues;
  fieldErrors: BodyMetricFieldErrors;
  editingEntry: BodyMetricEntry | null;
  submitError: string | null;
  submitSuccess: string | null;
  isSubmitting: boolean;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  onCancelEdit: () => void;
  onFieldChange: <TField extends keyof BodyMetricFormValues>(
    field: TField,
    value: BodyMetricFormValues[TField],
  ) => void;
}

export function BodyMetricForm({
  values,
  fieldErrors,
  editingEntry,
  submitError,
  submitSuccess,
  isSubmitting,
  onSubmit,
  onCancelEdit,
  onFieldChange,
}: BodyMetricFormProps) {
  return (
    <form onSubmit={onSubmit} className="space-y-4 rounded-2xl border border-border bg-white p-4">
      <div>
        <h2 className="text-lg font-semibold text-foreground">
          {editingEntry ? "Редагувати запис" : "Додати запис"}
        </h2>
        <p className="mt-1 text-sm text-muted">
          {editingEntry
            ? formatBodyMetricDate(editingEntry.recordedAt)
            : "Дані прогресу за обрану дату"}
        </p>
      </div>

      <div className="space-y-2">
        <label className={fieldLabelClassName} htmlFor="body-metric-recorded-at">
          Дата
        </label>
        <input
          id="body-metric-recorded-at"
          type="date"
          value={values.recordedAt}
          onChange={(event) => onFieldChange("recordedAt", event.currentTarget.value)}
          disabled={isSubmitting}
          className={fieldInputClassName}
        />
        {fieldErrors.recordedAt ? (
          <p className={fieldErrorClassName}>{fieldErrors.recordedAt}</p>
        ) : null}
      </div>

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-1">
        {metricFields.map((field) => (
          <div key={field.name} className="space-y-2">
            <label className={fieldLabelClassName} htmlFor={`body-metric-${field.name}`}>
              {field.label}, {field.suffix}
            </label>
            <input
              id={`body-metric-${field.name}`}
              type="number"
              min={1}
              max={field.max}
              step={field.step}
              value={values[field.name]}
              onChange={(event) => onFieldChange(field.name, event.currentTarget.value)}
              disabled={isSubmitting}
              className={fieldInputClassName}
            />
            {fieldErrors[field.name] ? (
              <p className={fieldErrorClassName}>{fieldErrors[field.name]}</p>
            ) : null}
          </div>
        ))}
      </div>

      <div className="space-y-2">
        <label className={fieldLabelClassName} htmlFor="body-metric-note">
          Нотатка
        </label>
        <textarea
          id="body-metric-note"
          value={values.note}
          onChange={(event) => onFieldChange("note", event.currentTarget.value)}
          disabled={isSubmitting}
          rows={4}
          maxLength={1000}
          className={`${fieldInputClassName} resize-y`}
        />
        {fieldErrors.note ? <p className={fieldErrorClassName}>{fieldErrors.note}</p> : null}
      </div>

      <FormAlert message={submitError} />

      {submitSuccess ? (
        <p
          role="status"
          className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800"
        >
          {submitSuccess}
        </p>
      ) : null}

      <div className="flex flex-wrap gap-2">
        <button
          type="submit"
          disabled={isSubmitting}
          className="inline-flex min-h-10 items-center justify-center rounded-lg bg-accent px-5 py-2 text-sm font-semibold text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isSubmitting ? "Зберігаємо..." : editingEntry ? "Оновити" : "Додати"}
        </button>

        {editingEntry ? (
          <button
            type="button"
            onClick={onCancelEdit}
            disabled={isSubmitting}
            className="inline-flex min-h-10 items-center justify-center rounded-lg border border-border px-5 py-2 text-sm font-semibold text-foreground transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
          >
            Скасувати
          </button>
        ) : null}
      </div>
    </form>
  );
}
