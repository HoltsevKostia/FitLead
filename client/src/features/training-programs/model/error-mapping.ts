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

  return error.detail ?? error.title ?? "Не вдалося виконати дію. Спробуйте ще раз.";
}
