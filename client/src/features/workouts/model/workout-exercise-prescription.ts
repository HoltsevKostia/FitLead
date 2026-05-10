export interface WorkoutExercisePrescriptionInput {
  sets: string;
  repetitions: string;
  loadKg: string;
  restSeconds: string;
  trainerNote: string;
}

export interface WorkoutExercisePrescriptionPayload {
  repetitions: number;
  sets: number;
  loadKg: number | null;
  restSeconds: number;
  trainerNote: string | null;
}

export interface ParseWorkoutExercisePrescriptionResult {
  payload: WorkoutExercisePrescriptionPayload | null;
  error: string | null;
}

function parseRequiredPositiveInt(value: string): number | null {
  const parsed = Number(value);

  if (!Number.isInteger(parsed) || parsed <= 0) {
    return null;
  }

  return parsed;
}

function parseNonNegativeInt(value: string): number | null {
  const parsed = Number(value);

  if (!Number.isInteger(parsed) || parsed < 0) {
    return null;
  }

  return parsed;
}

function parseOptionalNonNegativeNumber(value: string): number | null | undefined {
  const trimmed = value.trim();

  if (!trimmed) {
    return null;
  }

  const parsed = Number(trimmed);

  if (!Number.isFinite(parsed) || parsed < 0) {
    return undefined;
  }

  return parsed;
}

export function parseWorkoutExercisePrescription(
  input: WorkoutExercisePrescriptionInput,
): ParseWorkoutExercisePrescriptionResult {
  const sets = parseRequiredPositiveInt(input.sets);
  const repetitions = parseRequiredPositiveInt(input.repetitions);
  const restSeconds = parseNonNegativeInt(input.restSeconds);
  const loadKg = parseOptionalNonNegativeNumber(input.loadKg);

  if (!sets || !repetitions || restSeconds === null) {
    return {
      payload: null,
      error: "Перевірте підходи, повторення та відпочинок.",
    };
  }

  if (loadKg === undefined) {
    return {
      payload: null,
      error: "Вага має бути невід’ємним числом.",
    };
  }

  return {
    payload: {
      repetitions,
      sets,
      loadKg,
      restSeconds,
      trainerNote: input.trainerNote.trim() || null,
    },
    error: null,
  };
}
