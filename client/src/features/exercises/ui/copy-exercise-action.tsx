"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";

import { ExerciseSource, type Exercise } from "@/entities/exercise/model/types";
import { mapExerciseMutationError } from "@/features/exercises/model/error-mapping";
import { exercisesApi } from "@/lib/api/clients/exercises-api";
import { FormAlert } from "@/shared/forms/form-alert";

interface CopyExerciseActionProps {
  exercise: Exercise;
}

export function CopyExerciseAction({ exercise }: CopyExerciseActionProps) {
  const router = useRouter();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  if (exercise.source !== ExerciseSource.Platform) {
    return null;
  }

  async function handleCopy() {
    setIsSubmitting(true);
    setSubmitError(null);

    try {
      await exercisesApi.copyToMyLibrary(exercise.id);
      router.refresh();
    } catch (error) {
      setSubmitError(mapExerciseMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="w-full space-y-2 md:w-auto">
      <button
        type="button"
        onClick={handleCopy}
        disabled={isSubmitting}
        className="rounded-full border border-accent px-4 py-2 text-sm font-medium text-accent transition hover:bg-emerald-50 disabled:cursor-not-allowed disabled:opacity-70"
      >
        {isSubmitting ? "Копіюємо..." : "Додати до моїх вправ"}
      </button>
      <FormAlert message={submitError} />
    </div>
  );
}
