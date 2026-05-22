"use client";

import { type SubmitEvent, useRef, useState } from "react";
import { useRouter } from "next/navigation";

import { Equipment, MuscleGroup, type Exercise } from "@/entities/exercise/model/types";
import type { MediaAssetPreview } from "@/entities/media-asset/model/types";
import {
  equipmentOptions,
  formatOptionalNumber,
  muscleGroupOptions,
  parseOptionalNumber,
} from "@/features/exercises/model/exercise-form-options";
import {
  equipmentLabels,
  muscleGroupLabels,
} from "@/features/exercises/model/exercise-labels";
import { mapExerciseMutationError } from "@/features/exercises/model/error-mapping";
import {
  type ExerciseDeleteConflict,
  readExerciseDeleteConflict,
} from "@/features/exercises/model/delete-conflict";
import {
  ExerciseMediaUploader,
  type ExerciseMediaUploaderRef,
} from "@/features/exercises/ui/exercise-media-uploader";
import { ExerciseMediaPreview } from "@/features/exercises/ui/exercise-media-preview";
import { ExerciseDeleteConfirmation } from "@/features/exercises/ui/exercise-delete-confirmation";
import { isApiError } from "@/lib/api/api-error";
import { exercisesApi } from "@/lib/api/clients/exercises-api";
import { FormAlert } from "@/shared/forms/form-alert";
import { fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";

interface ExerciseActionsProps {
  exercise: Exercise;
}

export function ExerciseActions({ exercise }: ExerciseActionsProps) {
  const router = useRouter();
  const uploaderRef = useRef<ExerciseMediaUploaderRef>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [name, setName] = useState(exercise.name);
  const [description, setDescription] = useState(exercise.description);
  const [fileCount, setFileCount] = useState(0);
  const [selectedMediaAsset, setSelectedMediaAsset] =
    useState<MediaAssetPreview | null>(exercise.mediaAsset);
  const [muscleGroup, setMuscleGroup] = useState(formatOptionalNumber(exercise.muscleGroup));
  const [equipment, setEquipment] = useState(formatOptionalNumber(exercise.equipment));
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [deleteConflict, setDeleteConflict] = useState<ExerciseDeleteConflict | null>(null);
  const [isConfirmingDelete, setIsConfirmingDelete] = useState(false);

  if (!exercise.isEditable) {
    return null;
  }

  async function handleUpdate() {
    const trimmedName = name.trim();

    if (!trimmedName) {
      setSubmitError("Вкажіть назву вправи.");
      setDeleteConflict(null);
      return;
    }

    setIsSubmitting(true);
    setSubmitError(null);
    setDeleteConflict(null);

    try {
      const uploadedMedia = await uploaderRef.current?.uploadSelectedMedia();
      const mediaAsset = uploadedMedia ?? selectedMediaAsset;

      if (uploadedMedia) {
        setSelectedMediaAsset(uploadedMedia);
      }

      await exercisesApi.updateExercise(exercise.id, {
        name: trimmedName,
        description: description.trim(),
        mediaAssetId: mediaAsset?.id ?? null,
        muscleGroup: parseOptionalNumber<MuscleGroup>(muscleGroup),
        equipment: parseOptionalNumber<Equipment>(equipment),
      });
      setFileCount(0);
      uploaderRef.current?.clear();
      setIsEditing(false);
      router.refresh();
    } catch (error) {
      setSubmitError(mapExerciseMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDelete() {
    setIsSubmitting(true);
    setSubmitError(null);
    setDeleteConflict(null);

    try {
      await exercisesApi.deleteExercise(exercise.id);
      router.refresh();
    } catch (error) {
      const conflict = readExerciseDeleteConflict(error);
      if (conflict) {
        setDeleteConflict(conflict);
        return;
      }

      setSubmitError(mapExerciseMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleConfirmDelete() {
    if (!deleteConflict) {
      return;
    }

    setIsConfirmingDelete(true);
    setSubmitError(null);

    try {
      await exercisesApi.confirmDeleteExercise(exercise.id, deleteConflict.confirmationToken);
      setDeleteConflict(null);
      router.refresh();
    } catch (error) {
      const conflict = readExerciseDeleteConflict(error);
      if (conflict) {
        setDeleteConflict(conflict);
        return;
      }

      if (
        isApiError(error) &&
        error.status === 400 &&
        error.errorCode === "exercise.delete.token_invalid"
      ) {
        setDeleteConflict(null);
        setSubmitError("Підтвердження застаріло. Повторіть видалення вправи.");
        return;
      }

      setSubmitError(mapExerciseMutationError(error));
    } finally {
      setIsConfirmingDelete(false);
    }
  }

  function handleCancelDeleteConfirmation() {
    setDeleteConflict(null);
    setSubmitError(null);
  }

  function handleEditToggle() {
    setIsEditing((current) => {
      const next = !current;

      if (!next) {
        setName(exercise.name);
        setDescription(exercise.description);
        setSelectedMediaAsset(exercise.mediaAsset);
        setFileCount(0);
        uploaderRef.current?.clear();
        setMuscleGroup(formatOptionalNumber(exercise.muscleGroup));
        setEquipment(formatOptionalNumber(exercise.equipment));
      }

      return next;
    });
    setSubmitError(null);
    setDeleteConflict(null);
  }

  function handleEditSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    void handleUpdate();
  }

  return (
    <div className="relative w-full space-y-3 md:w-auto">
      <div className="flex shrink-0 flex-wrap gap-2 md:justify-end">
        <button
          type="button"
          onClick={handleEditToggle}
          disabled={isSubmitting || isConfirmingDelete}
          className="rounded-full border border-border px-4 py-2 text-sm font-medium transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isEditing ? "Скасувати" : "Редагувати"}
        </button>
        <button
          type="button"
          onClick={handleDelete}
          disabled={isSubmitting || isConfirmingDelete}
          className="rounded-full border border-red-200 px-4 py-2 text-sm font-medium text-red-700 transition hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isSubmitting ? "Обробка..." : "Видалити"}
        </button>
      </div>

      <FormAlert message={submitError} />

      {deleteConflict ? (
        <ExerciseDeleteConfirmation
          conflict={deleteConflict}
          isConfirming={isConfirmingDelete}
          onConfirm={handleConfirmDelete}
          onCancel={handleCancelDeleteConfirmation}
        />
      ) : null}

      {isEditing ? (
        <form
          onSubmit={handleEditSubmit}
          className="space-y-3 rounded-2xl border border-border bg-surface px-4 py-4"
        >
          <div className="space-y-2">
            <label className={fieldLabelClassName} htmlFor={`exercise-name-${exercise.id}`}>
              Назва
            </label>
            <input
              id={`exercise-name-${exercise.id}`}
              value={name}
              onChange={(event) => setName(event.target.value)}
              disabled={isSubmitting}
              className={fieldInputClassName}
            />
          </div>

          <div className="space-y-2">
            <label className={fieldLabelClassName} htmlFor={`exercise-description-${exercise.id}`}>
              Опис
            </label>
            <textarea
              id={`exercise-description-${exercise.id}`}
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              disabled={isSubmitting}
              rows={4}
              className={`${fieldInputClassName} resize-y`}
            />
          </div>

          <div className="space-y-3">
            <p className={fieldLabelClassName}>Медіа</p>
            {selectedMediaAsset ? (
              <div className="flex flex-wrap items-center gap-3 rounded-2xl border border-border bg-white px-4 py-3">
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

          <div className="grid gap-3 md:grid-cols-2">
            <div className="space-y-2">
              <label className={fieldLabelClassName} htmlFor={`exercise-muscle-group-${exercise.id}`}>
                Група м&apos;язів
              </label>
              <select
                id={`exercise-muscle-group-${exercise.id}`}
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
              <label className={fieldLabelClassName} htmlFor={`exercise-equipment-${exercise.id}`}>
                Обладнання
              </label>
              <select
                id={`exercise-equipment-${exercise.id}`}
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

          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
          >
            {isSubmitting ? "Зберігаємо..." : "Зберегти"}
          </button>
        </form>
      ) : null}
    </div>
  );
}
