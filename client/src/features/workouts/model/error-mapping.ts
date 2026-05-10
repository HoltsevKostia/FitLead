import { isApiError } from "@/lib/api/api-error";

export function mapWorkoutMutationError(error: unknown): string {
  if (!isApiError(error)) {
    return "Не вдалося виконати дію. Спробуйте ще раз.";
  }

  if (error.errorCode === "workout.create.name_required") {
    return "Вкажіть назву тренування.";
  }

  if (error.errorCode === "exercise.not_found") {
    return "Вправу не знайдено або вона недоступна.";
  }

  if (
    error.errorCode === "workout.exercise.create.invalid_reps_or_sets" ||
    error.errorCode === "workout.exercise.update.invalid_reps_or_sets"
  ) {
    return "Підходи та повторення мають бути більшими за нуль.";
  }

  if (
    error.errorCode === "workout.exercise.create.rest_seconds_negative" ||
    error.errorCode === "workout.exercise.update.rest_seconds_negative"
  ) {
    return "Відпочинок не може бути від’ємним.";
  }

  if (
    error.errorCode === "workout.exercise.create.load_kg_negative" ||
    error.errorCode === "workout.exercise.update.load_kg_negative"
  ) {
    return "Вага не може бути від’ємною.";
  }

  if (
    error.errorCode === "workout.exercise.create.trainer_note_too_long" ||
    error.errorCode === "workout.exercise.update.trainer_note_too_long"
  ) {
    return "Нотатка тренера має бути не довшою за 1000 символів.";
  }

  return error.detail ?? error.title ?? "Не вдалося виконати дію. Спробуйте ще раз.";
}
