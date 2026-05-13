export function getSafeReturnPath(value: string | undefined, fallback: string): string {
  if (!value || !value.startsWith("/") || value.startsWith("//")) {
    return fallback;
  }

  return value;
}

export function buildClientWorkoutPath(
  assignmentId: string,
  workoutId: string,
  returnTo?: string,
): string {
  const path = `/client/training-programs/${assignmentId}/workouts/${workoutId}`;

  if (!returnTo) {
    return path;
  }

  return `${path}?returnTo=${encodeURIComponent(returnTo)}`;
}

export function buildClientExercisePath(
  assignmentId: string,
  workoutId: string,
  workoutExerciseId: string,
  returnTo?: string,
): string {
  const path = `/client/training-programs/${assignmentId}/workouts/${workoutId}/exercises/${workoutExerciseId}`;

  if (!returnTo) {
    return path;
  }

  return `${path}?returnTo=${encodeURIComponent(returnTo)}`;
}
