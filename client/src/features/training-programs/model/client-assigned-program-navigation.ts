export function getSafeReturnPath(value: string | undefined, fallback: string): string {
  if (!value || !value.startsWith("/") || value.startsWith("//")) {
    return fallback;
  }

  return value;
}

export function buildClientWorkoutPath(
  assignmentId: string,
  programWorkoutId: string,
  returnTo?: string,
): string {
  const path = `/client/training-programs/${assignmentId}/workouts/${programWorkoutId}`;

  if (!returnTo) {
    return path;
  }

  return `${path}?returnTo=${encodeURIComponent(returnTo)}`;
}

export function buildClientExercisePath(
  assignmentId: string,
  programWorkoutId: string,
  workoutExerciseId: string,
  returnTo?: string,
): string {
  const path = `/client/training-programs/${assignmentId}/workouts/${programWorkoutId}/exercises/${workoutExerciseId}`;

  if (!returnTo) {
    return path;
  }

  return `${path}?returnTo=${encodeURIComponent(returnTo)}`;
}
