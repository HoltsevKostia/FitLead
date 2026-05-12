import { isApiError } from "@/lib/api/api-error";

export function mapTrainingProgramMutationError(error: unknown): string {
  if (!isApiError(error)) {
    return "Не вдалося виконати дію. Спробуйте ще раз.";
  }

  if (error.errorCode === "training_program.create.title_required") {
    return "Вкажіть назву програми.";
  }

  if (error.errorCode === "training_program.create.weeks_count_out_of_range") {
    return "Кількість тижнів має бути від 1 до 24.";
  }

  if (error.errorCode === "training_program.create.days_per_week_out_of_range") {
    return "Кількість днів у тижні має бути від 1 до 7.";
  }

  if (error.errorCode === "training_program.workouts.week_number_out_of_range") {
    return "Тиждень поза межами структури програми.";
  }

  if (error.errorCode === "training_program.workouts.day_number_out_of_range") {
    return "День поза межами структури програми.";
  }

  if (error.errorCode === "training_program.workouts.remove.not_found") {
    return "Запис тренування у програмі не знайдено.";
  }

  if (error.errorCode === "training_program.workouts.remove.entry_id_required") {
    return "Не вдалося визначити запис тренування.";
  }

  if (error.errorCode === "training_program.not_found") {
    return "Програму не знайдено.";
  }

  if (error.errorCode === "assignment.already_exists") {
    return "Цей клієнт уже має активний доступ до програми.";
  }

  if (error.errorCode === "client.not_found") {
    return "Клієнта не знайдено серед ваших клієнтів.";
  }

  if (error.errorCode === "training_program.assignment.client_id_required") {
    return "Виберіть клієнта.";
  }

  if (error.errorCode === "training_program.assignment.create.expires_at_invalid") {
    return "Дата завершення має бути у майбутньому.";
  }

  if (error.errorCode === "workout.not_found") {
    return "Тренування не знайдено.";
  }

  if (error.errorCode === "workout.forbidden") {
    return "Це тренування належить іншому тренеру.";
  }

  return error.detail ?? error.title ?? "Не вдалося виконати дію. Спробуйте ще раз.";
}
