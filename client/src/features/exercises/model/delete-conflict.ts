import { isApiError } from "@/lib/api/api-error";

export interface ExerciseDeleteConflict {
  workoutExerciseCount: number;
  confirmationToken: string;
}

function isObject(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

export function readExerciseDeleteConflict(error: unknown): ExerciseDeleteConflict | null {
  if (!isApiError(error) || error.status !== 409 || error.errorCode !== "exercise.in_use") {
    return null;
  }

  const usage = error.extensions.usage;
  const confirmationToken = error.extensions.confirmationToken;

  if (!isObject(usage) || typeof confirmationToken !== "string") {
    return null;
  }

  const workoutExerciseCount = usage.workoutExerciseCount;

  if (typeof workoutExerciseCount !== "number") {
    return null;
  }

  return {
    workoutExerciseCount,
    confirmationToken,
  };
}
