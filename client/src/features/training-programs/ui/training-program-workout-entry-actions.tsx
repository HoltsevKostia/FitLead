"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

import { mapTrainingProgramMutationError } from "@/features/training-programs/model/error-mapping";
import { trainingProgramsApi } from "@/lib/api/clients/training-programs-api";
import { FormAlert } from "@/shared/forms/form-alert";

interface TrainingProgramWorkoutEntryActionsProps {
  programId: string;
  entryId: string;
}

export function TrainingProgramWorkoutEntryActions({
  programId,
  entryId,
}: TrainingProgramWorkoutEntryActionsProps) {
  const router = useRouter();
  const [isRemoving, setIsRemoving] = useState(false);
  const [removeError, setRemoveError] = useState<string | null>(null);

  async function handleRemove() {
    setIsRemoving(true);
    setRemoveError(null);

    try {
      await trainingProgramsApi.removeWorkout(programId, entryId);
      router.refresh();
    } catch (error) {
      setRemoveError(mapTrainingProgramMutationError(error));
    } finally {
      setIsRemoving(false);
    }
  }

  return (
    <div className="space-y-2">
      <button
        type="button"
        onClick={handleRemove}
        disabled={isRemoving}
        className="w-fit rounded-full border border-red-200 px-3 py-2 text-sm font-medium text-red-700 transition hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-70"
      >
        {isRemoving ? "Прибираємо..." : "Прибрати"}
      </button>
      <FormAlert message={removeError} />
    </div>
  );
}
