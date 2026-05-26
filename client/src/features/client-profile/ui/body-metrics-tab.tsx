"use client";

import { type FormEvent, useEffect, useMemo, useRef, useState } from "react";

import type { BodyMetricEntry } from "@/entities/body-metric/model/types";
import { mapBodyMetricMutationError } from "@/features/client-profile/model/body-metric-error-mapping";
import { bodyMetricsApi } from "@/lib/api/clients/body-metrics-api";
import { FormAlert } from "@/shared/forms/form-alert";

import { BodyMetricEntryCard } from "./body-metric-entry-card";
import { BodyMetricForm } from "./body-metric-form";
import {
  type BodyMetricFieldErrors,
  type BodyMetricFormValues,
  createEmptyValues,
  createPrefilledValues,
  getInitialValues,
  toBodyMetricRequest,
  validateBodyMetricForm,
} from "./body-metric-form-utils";

export function BodyMetricsTab() {
  const [entries, setEntries] = useState<BodyMetricEntry[]>([]);
  const [values, setValues] = useState<BodyMetricFormValues>(() => createEmptyValues());
  const [fieldErrors, setFieldErrors] = useState<BodyMetricFieldErrors>({});
  const [editingEntryId, setEditingEntryId] = useState<string | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitSuccess, setSubmitSuccess] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [deletingEntryId, setDeletingEntryId] = useState<string | null>(null);
  const isFormDirtyRef = useRef(false);

  const editingEntry = useMemo(
    () => entries.find((entry) => entry.id === editingEntryId) ?? null,
    [editingEntryId, entries],
  );

  useEffect(() => {
    let ignore = false;

    async function loadEntries() {
      setIsLoading(true);
      setLoadError(null);

      try {
        const loadedEntries = await bodyMetricsApi.list();
        if (!ignore) {
          setEntries(loadedEntries);
          if (!isFormDirtyRef.current) {
            setValues(createPrefilledValues(loadedEntries));
          }
        }
      } catch (error) {
        if (!ignore) {
          setLoadError(mapBodyMetricMutationError(error));
        }
      } finally {
        if (!ignore) {
          setIsLoading(false);
        }
      }
    }

    void loadEntries();

    return () => {
      ignore = true;
    };
  }, []);

  function updateField<TField extends keyof BodyMetricFormValues>(
    field: TField,
    value: BodyMetricFormValues[TField],
  ) {
    isFormDirtyRef.current = true;
    setValues((currentValues) => ({
      ...currentValues,
      [field]: value,
    }));

    if (fieldErrors[field]) {
      setFieldErrors((currentErrors) => ({
        ...currentErrors,
        [field]: undefined,
      }));
    }

    setSubmitError(null);
    setSubmitSuccess(null);
  }

  function startEdit(entry: BodyMetricEntry) {
    isFormDirtyRef.current = true;
    setEditingEntryId(entry.id);
    setValues(getInitialValues(entry));
    setFieldErrors({});
    setSubmitError(null);
    setSubmitSuccess(null);
  }

  function resetForm() {
    isFormDirtyRef.current = false;
    setEditingEntryId(null);
    setValues(createPrefilledValues(entries));
    setFieldErrors({});
    setSubmitError(null);
    setSubmitSuccess(null);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const validationErrors = validateBodyMetricForm(values);
    if (Object.keys(validationErrors).length > 0) {
      setFieldErrors(validationErrors);
      setSubmitSuccess(null);
      return;
    }

    setIsSubmitting(true);
    setFieldErrors({});
    setSubmitError(null);
    setSubmitSuccess(null);

    try {
      const request = toBodyMetricRequest(values);
      const savedEntry = editingEntryId
        ? await bodyMetricsApi.update(editingEntryId, request)
        : await bodyMetricsApi.create(request);

      const nextEntries = [savedEntry, ...entries.filter((entry) => entry.id !== savedEntry.id)]
        .sort((left, right) => right.recordedAt.localeCompare(left.recordedAt));
      setEntries(nextEntries);
      isFormDirtyRef.current = false;
      setEditingEntryId(null);
      setValues(createPrefilledValues(nextEntries));
      setSubmitSuccess(editingEntryId ? "Запис оновлено." : "Запис додано.");
    } catch (error) {
      setSubmitError(mapBodyMetricMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDelete(entry: BodyMetricEntry) {
    if (!window.confirm("Видалити запис метрик?")) {
      return;
    }

    setDeletingEntryId(entry.id);
    setSubmitError(null);
    setSubmitSuccess(null);

    try {
      await bodyMetricsApi.delete(entry.id);
      const nextEntries = entries.filter((current) => current.id !== entry.id);
      setEntries(nextEntries);

      if (editingEntryId === entry.id) {
        isFormDirtyRef.current = false;
        setEditingEntryId(null);
        setValues(createPrefilledValues(nextEntries));
        setFieldErrors({});
        setSubmitError(null);
        setSubmitSuccess(null);
      } else if (!isFormDirtyRef.current) {
        setValues(createPrefilledValues(nextEntries));
      }
    } catch (error) {
      setSubmitError(mapBodyMetricMutationError(error));
    } finally {
      setDeletingEntryId(null);
    }
  }

  return (
    <div className="grid gap-6 xl:grid-cols-[minmax(0,360px)_minmax(0,1fr)]">
      <BodyMetricForm
        values={values}
        fieldErrors={fieldErrors}
        editingEntry={editingEntry}
        submitError={submitError}
        submitSuccess={submitSuccess}
        isSubmitting={isSubmitting}
        onSubmit={handleSubmit}
        onCancelEdit={resetForm}
        onFieldChange={updateField}
      />

      <div className="space-y-4">
        <div>
          <h2 className="text-lg font-semibold text-foreground">Історія метрик</h2>
          <p className="mt-1 text-sm text-muted">Записи відсортовані від найновішого до найстарішого.</p>
        </div>

        <FormAlert message={loadError} />

        {isLoading ? (
          <p className="rounded-2xl border border-border bg-surface-strong/50 px-4 py-5 text-sm text-muted">
            Завантажуємо метрики...
          </p>
        ) : entries.length === 0 ? (
          <p className="rounded-2xl border border-border bg-surface-strong/50 px-4 py-5 text-sm text-muted">
            Записів метрик ще немає.
          </p>
        ) : (
          <div className="space-y-3">
            {entries.map((entry) => (
              <BodyMetricEntryCard
                key={entry.id}
                entry={entry}
                isDeleting={deletingEntryId === entry.id}
                onEdit={startEdit}
                onDelete={(selectedEntry) => void handleDelete(selectedEntry)}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
