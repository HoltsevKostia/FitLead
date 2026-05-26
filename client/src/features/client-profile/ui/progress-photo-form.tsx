"use client";

import { type FormEvent, useRef, useState } from "react";

import type {
  CreateProgressPhotoRequest,
  ProgressPhoto,
  ProgressPhotoLabel,
} from "@/entities/progress-photo/model/types";
import { mapProgressPhotoMutationError } from "@/features/client-profile/model/progress-photo-error-mapping";
import {
  ProgressPhotoImageUploader,
  type ProgressPhotoImageUploaderRef,
} from "@/features/client-profile/ui/progress-photo-image-uploader";
import { progressPhotosApi } from "@/lib/api/clients/progress-photos-api";
import { FormAlert } from "@/shared/forms/form-alert";
import {
  fieldErrorClassName,
  fieldInputClassName,
  fieldLabelClassName,
} from "@/shared/forms/field-styles";

interface ProgressPhotoFormProps {
  onCreated?: (photo: ProgressPhoto) => void;
}

interface ProgressPhotoFormValues {
  takenAt: string;
  label: ProgressPhotoLabel;
  note: string;
}

type ProgressPhotoFieldErrors = Partial<Record<keyof ProgressPhotoFormValues | "image", string>>;

const labelOptions: Array<{ value: ProgressPhotoLabel; label: string }> = [
  { value: "Front", label: "Спереду" },
  { value: "Side", label: "Збоку" },
  { value: "Back", label: "Ззаду" },
  { value: "Other", label: "Інше" },
];

function getTodayDateInputValue(): string {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, "0");
  const day = String(now.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function createInitialValues(): ProgressPhotoFormValues {
  return {
    takenAt: getTodayDateInputValue(),
    label: "Front",
    note: "",
  };
}

function normalizeOptionalText(value: string): string | null {
  const trimmedValue = value.trim();
  return trimmedValue ? trimmedValue : null;
}

function validateForm(
  values: ProgressPhotoFormValues,
  fileCount: number,
): ProgressPhotoFieldErrors {
  const errors: ProgressPhotoFieldErrors = {};

  if (!values.takenAt) {
    errors.takenAt = "Оберіть дату фото.";
  }

  if (!labelOptions.some((option) => option.value === values.label)) {
    errors.label = "Оберіть тип фото.";
  }

  if (values.note.trim().length > 1000) {
    errors.note = "Нотатка має бути не довшою за 1000 символів.";
  }

  if (fileCount === 0) {
    errors.image = "Додайте фото.";
  }

  return errors;
}

function toRequest(
  values: ProgressPhotoFormValues,
  mediaAssetId: string,
): CreateProgressPhotoRequest {
  return {
    mediaAssetId,
    takenAt: values.takenAt,
    label: values.label,
    note: normalizeOptionalText(values.note),
  };
}

export function ProgressPhotoForm({ onCreated }: ProgressPhotoFormProps) {
  const uploaderRef = useRef<ProgressPhotoImageUploaderRef>(null);
  const [values, setValues] = useState<ProgressPhotoFormValues>(() => createInitialValues());
  const [fieldErrors, setFieldErrors] = useState<ProgressPhotoFieldErrors>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitSuccess, setSubmitSuccess] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [fileCount, setFileCount] = useState(0);

  function updateField<TField extends keyof ProgressPhotoFormValues>(
    field: TField,
    value: ProgressPhotoFormValues[TField],
  ) {
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

  function handleFileCountChange(nextFileCount: number) {
    setFileCount(nextFileCount);

    if (fieldErrors.image) {
      setFieldErrors((currentErrors) => ({
        ...currentErrors,
        image: undefined,
      }));
    }

    setSubmitError(null);
    setSubmitSuccess(null);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const validationErrors = validateForm(values, fileCount);
    if (Object.keys(validationErrors).length > 0) {
      setFieldErrors(validationErrors);
      setSubmitError(null);
      setSubmitSuccess(null);
      return;
    }

    setIsSubmitting(true);
    setFieldErrors({});
    setSubmitError(null);
    setSubmitSuccess(null);

    try {
      const mediaAsset = await uploaderRef.current?.uploadSelectedImage();
      if (!mediaAsset) {
        setFieldErrors({
          image: "Не вдалося завантажити фото. Спробуйте ще раз.",
        });
        return;
      }

      const createdPhoto = await progressPhotosApi.create(toRequest(values, mediaAsset.id));
      onCreated?.(createdPhoto);
      setValues(createInitialValues());
      uploaderRef.current?.clear();
      setFileCount(0);
      setSubmitSuccess("Фото прогресу додано.");
    } catch (error) {
      setSubmitError(mapProgressPhotoMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="max-w-3xl space-y-5">
      <div className="grid gap-4 md:grid-cols-2">
        <div className="space-y-2">
          <label className={fieldLabelClassName} htmlFor="progress-photo-taken-at">
            Дата
          </label>
          <input
            id="progress-photo-taken-at"
            type="date"
            value={values.takenAt}
            onChange={(event) => updateField("takenAt", event.currentTarget.value)}
            disabled={isSubmitting}
            className={fieldInputClassName}
          />
          {fieldErrors.takenAt ? (
            <p className={fieldErrorClassName}>{fieldErrors.takenAt}</p>
          ) : null}
        </div>

        <div className="space-y-2">
          <label className={fieldLabelClassName} htmlFor="progress-photo-label">
            Тип фото
          </label>
          <select
            id="progress-photo-label"
            value={values.label}
            onChange={(event) =>
              updateField("label", event.currentTarget.value as ProgressPhotoLabel)
            }
            disabled={isSubmitting}
            className={fieldInputClassName}
          >
            {labelOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
          {fieldErrors.label ? (
            <p className={fieldErrorClassName}>{fieldErrors.label}</p>
          ) : null}
        </div>

        <div className="space-y-2 md:col-span-2">
          <label className={fieldLabelClassName}>Фото</label>
          <ProgressPhotoImageUploader
            ref={uploaderRef}
            onFileCountChange={handleFileCountChange}
            onUploadFailed={(message) => setSubmitError(message)}
          />
          {fieldErrors.image ? (
            <p className={fieldErrorClassName}>{fieldErrors.image}</p>
          ) : null}
        </div>

        <div className="space-y-2 md:col-span-2">
          <label className={fieldLabelClassName} htmlFor="progress-photo-note">
            Нотатка
          </label>
          <textarea
            id="progress-photo-note"
            value={values.note}
            onChange={(event) => updateField("note", event.currentTarget.value)}
            disabled={isSubmitting}
            rows={4}
            maxLength={1000}
            className={`${fieldInputClassName} resize-y`}
          />
          {fieldErrors.note ? <p className={fieldErrorClassName}>{fieldErrors.note}</p> : null}
        </div>
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

      <button
        type="submit"
        disabled={isSubmitting}
        className="inline-flex min-h-10 items-center justify-center rounded-lg bg-accent px-5 py-2 text-sm font-semibold text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
      >
        {isSubmitting ? "Зберігаємо..." : "Додати фото"}
      </button>
    </form>
  );
}
