"use client";

import { type SubmitEvent, useRef, useState } from "react";
import { useRouter } from "next/navigation";

import { Equipment, MuscleGroup } from "@/entities/exercise/model/types";
import type { MediaAssetPreview } from "@/entities/media-asset/model/types";
import {
  equipmentOptions,
  muscleGroupOptions,
  parseOptionalNumber,
} from "@/features/exercises/model/exercise-form-options";
import {
  equipmentLabels,
  muscleGroupLabels,
} from "@/features/exercises/model/exercise-labels";
import { mapExerciseMutationError } from "@/features/exercises/model/error-mapping";
import {
  ExerciseMediaUploader,
  type ExerciseMediaUploaderRef,
} from "@/features/exercises/ui/exercise-media-uploader";
import { ExerciseMediaPreview } from "@/features/exercises/ui/exercise-media-preview";
import { exercisesApi } from "@/lib/api/clients/exercises-api";
import { FormAlert } from "@/shared/forms/form-alert";
import { fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";

interface CreateExerciseFormProps {
  onCreated: () => void;
  onCancel: () => void;
}

export function CreateExerciseForm({ onCreated, onCancel }: CreateExerciseFormProps) {
  const router = useRouter();
  const uploaderRef = useRef<ExerciseMediaUploaderRef>(null);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [fileCount, setFileCount] = useState(0);
  const [selectedMediaAsset, setSelectedMediaAsset] =
    useState<MediaAssetPreview | null>(null);
  const [muscleGroup, setMuscleGroup] = useState("");
  const [equipment, setEquipment] = useState("");
  const [nameError, setNameError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function resetForm() {
    setName("");
    setDescription("");
    setFileCount(0);
    setSelectedMediaAsset(null);
    uploaderRef.current?.clear();
    setMuscleGroup("");
    setEquipment("");
    setNameError(null);
    setSubmitError(null);
  }

  function handleCancel() {
    resetForm();
    onCancel();
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    const trimmedName = name.trim();
    if (!trimmedName) {
      setNameError("Вкажіть назву вправи.");
      return;
    }

    setIsSubmitting(true);
    setNameError(null);
    setSubmitError(null);

    try {
      const uploadedMedia = await uploaderRef.current?.uploadSelectedMedia();
      const mediaAsset = uploadedMedia ?? selectedMediaAsset;

      if (uploadedMedia) {
        setSelectedMediaAsset(uploadedMedia);
      }

      await exercisesApi.createExercise({
        name: trimmedName,
        description: description.trim(),
        mediaAssetId: mediaAsset?.id ?? null,
        muscleGroup: parseOptionalNumber<MuscleGroup>(muscleGroup),
        equipment: parseOptionalNumber<Equipment>(equipment),
      });
      resetForm();
      router.refresh();
      onCreated();
    } catch (error) {
      setSubmitError(mapExerciseMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-4 rounded-2xl border border-border bg-white px-5 py-5"
    >
      <div className="grid gap-4 md:grid-cols-2">
        <div className="space-y-2 md:col-span-2">
          <label className={fieldLabelClassName} htmlFor="create-exercise-name">
            Назва
          </label>
          <input
            id="create-exercise-name"
            value={name}
            onChange={(event) => {
              setName(event.target.value);
              if (nameError) {
                setNameError(null);
              }
            }}
            disabled={isSubmitting}
            required
            maxLength={200}
            className={fieldInputClassName}
          />
          {nameError ? <p className="text-sm text-red-700">{nameError}</p> : null}
        </div>

        <div className="space-y-2 md:col-span-2">
          <label className={fieldLabelClassName} htmlFor="create-exercise-description">
            Опис
          </label>
          <textarea
            id="create-exercise-description"
            value={description}
            onChange={(event) => setDescription(event.target.value)}
            disabled={isSubmitting}
            rows={4}
            className={`${fieldInputClassName} resize-y`}
          />
        </div>

        <div className="space-y-3 md:col-span-2">
          <p className={fieldLabelClassName}>Медіа</p>
          {selectedMediaAsset ? (
            <div className="flex flex-wrap items-center gap-3 rounded-2xl border border-border bg-surface px-4 py-3">
              <ExerciseMediaPreview mediaAsset={selectedMediaAsset} />
              <button
                type="button"
                onClick={() => {
                  setSelectedMediaAsset(null);
                  uploaderRef.current?.clear();
                }}
                disabled={isSubmitting}
                className="rounded-full border border-border px-4 py-2 text-sm font-medium transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
              >
                Прибрати медіа
              </button>
            </div>
          ) : null}
          <ExerciseMediaUploader
            ref={uploaderRef}
            onFileCountChange={setFileCount}
            onUploadFailed={setSubmitError}
          />
          {fileCount > 0 ? (
            <button
              type="button"
              onClick={() => uploaderRef.current?.clear()}
              disabled={isSubmitting}
              className="rounded-full border border-border px-4 py-2 text-sm font-medium transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
            >
              Прибрати обраний файл
            </button>
          ) : null}
        </div>

        <div className="space-y-2">
          <label className={fieldLabelClassName} htmlFor="create-exercise-muscle-group">
            Група м'язів
          </label>
          <select
            id="create-exercise-muscle-group"
            value={muscleGroup}
            onChange={(event) => setMuscleGroup(event.target.value)}
            disabled={isSubmitting}
            className={fieldInputClassName}
          >
            <option value="">Не вказано</option>
            {muscleGroupOptions.map((option) => (
              <option key={option} value={option}>
                {muscleGroupLabels[option]}
              </option>
            ))}
          </select>
        </div>

        <div className="space-y-2">
          <label className={fieldLabelClassName} htmlFor="create-exercise-equipment">
            Обладнання
          </label>
          <select
            id="create-exercise-equipment"
            value={equipment}
            onChange={(event) => setEquipment(event.target.value)}
            disabled={isSubmitting}
            className={fieldInputClassName}
          >
            <option value="">Не вказано</option>
            {equipmentOptions.map((option) => (
              <option key={option} value={option}>
                {equipmentLabels[option]}
              </option>
            ))}
          </select>
        </div>
      </div>

      <FormAlert message={submitError} />

      <div className="flex flex-col gap-3 sm:flex-row">
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isSubmitting ? "Створюємо..." : "Створити"}
        </button>
        <button
          type="button"
          onClick={handleCancel}
          disabled={isSubmitting}
          className="rounded-full border border-border px-5 py-2 text-sm font-medium transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
        >
          Скасувати
        </button>
      </div>
    </form>
  );
}
