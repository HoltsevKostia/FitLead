import { Equipment, ExerciseSource, MuscleGroup } from "@/entities/exercise/model/types";

export const exerciseSourceLabels: Record<ExerciseSource, string> = {
  [ExerciseSource.Platform]: "Загальна бібліотека",
  [ExerciseSource.Trainer]: "Мої вправи",
};

export const exerciseSourceDescriptions: Record<ExerciseSource, string> = {
  [ExerciseSource.Platform]: "Загальна бібліотека вправ платформи",
  [ExerciseSource.Trainer]: "Власна вправа",
};

export const muscleGroupLabels: Record<MuscleGroup, string> = {
  [MuscleGroup.Chest]: "Груди",
  [MuscleGroup.Back]: "Спина",
  [MuscleGroup.Shoulders]: "Плечі",
  [MuscleGroup.Biceps]: "Біцепс",
  [MuscleGroup.Triceps]: "Трицепс",
  [MuscleGroup.Legs]: "Ноги",
  [MuscleGroup.Glutes]: "Сідниці",
  [MuscleGroup.Core]: "Кор",
  [MuscleGroup.FullBody]: "Все тіло",
  [MuscleGroup.Cardio]: "Кардіо",
};

export const equipmentLabels: Record<Equipment, string> = {
  [Equipment.Bodyweight]: "Власна вага",
  [Equipment.Dumbbells]: "Гантелі",
  [Equipment.Barbell]: "Штанга",
  [Equipment.Kettlebell]: "Гиря",
  [Equipment.Machine]: "Тренажер",
  [Equipment.Cable]: "Блок",
  [Equipment.ResistanceBand]: "Еспандер",
  [Equipment.Bench]: "Лава",
  [Equipment.PullUpBar]: "Турнік",
  [Equipment.Other]: "Інше",
};
