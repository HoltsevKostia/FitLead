"use client";

import { type SubmitEvent, useState } from "react";
import { useRouter } from "next/navigation";

import { Equipment, MuscleGroup } from "@/entities/exercise/model/types";
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
import { exercisesApi } from "@/lib/api/clients/exercises-api";
import { FormAlert } from "@/shared/forms/form-alert";
import { fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";

interface CreateExerciseFormProps {
  onCreated: () => void;
  onCancel: () => void;
}

export function CreateExerciseForm({ onCreated, onCancel }: CreateExerciseFormProps) {
  const router = useRouter();
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [mediaUrl, setMediaUrl] = useState("");
  const [muscleGroup, setMuscleGroup] = useState("");
  const [equipment, setEquipment] = useState("");
  const [nameError, setNameError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function resetForm() {
    setName("");
    setDescription("");
    setMediaUrl("");
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
      await exercisesApi.createExercise({
        name: trimmedName,
        description: description.trim(),
        mediaUrl: mediaUrl.trim() || null,
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

        <div className="space-y-2 md:col-span-2">
          <label className={fieldLabelClassName} htmlFor="create-exercise-media-url">
            Медіа-посилання
          </label>
          <input
            id="create-exercise-media-url"
            value={mediaUrl}
            onChange={(event) => setMediaUrl(event.target.value)}
            disabled={isSubmitting}
            type="url"
            maxLength={2048}
            placeholder="https://..."
            className={fieldInputClassName}
          />
        </div>

        <div className="space-y-2">
          <label className={fieldLabelClassName} htmlFor="create-exercise-muscle-group">
            Група м&apos;язів
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
