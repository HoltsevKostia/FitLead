import Link from "next/link";

import type { ClientAssignedTrainingProgramWorkout } from "@/entities/training-program/model/types";
import type { WorkoutExerciseDetails } from "@/entities/workout/model/types";
import {
  equipmentLabels,
  muscleGroupLabels,
} from "@/features/exercises/model/exercise-labels";
import { ExerciseDetailMedia } from "@/features/exercises/ui/exercise-detail-media";
import { PlainText } from "@/shared/ui/plain-text";

interface ClientAssignedExerciseDetailViewProps {
  workout: ClientAssignedTrainingProgramWorkout;
  exercise: WorkoutExerciseDetails;
  backHref: string;
}

function formatLoad(loadKg: number | null): string {
  if (loadKg === null) {
    return "Без ваги";
  }

  return `${loadKg} кг`;
}

export function ClientAssignedExerciseDetailView({
  workout,
  exercise,
  backHref,
}: ClientAssignedExerciseDetailViewProps) {
  return (
    <section className="space-y-6">
      <Link href={backHref} className="text-sm font-medium text-accent hover:text-accent-strong">
        Назад
      </Link>

      <div className="space-y-4">
        <div className="flex flex-wrap items-center gap-2">
          <span className="inline-flex rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
            {workout.workoutName}
          </span>
          <span className="inline-flex rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
            Вправа {exercise.order}
          </span>
          {exercise.exerciseMuscleGroup ? (
            <span className="inline-flex rounded-full border border-border bg-surface px-3 py-1 text-xs font-medium text-muted">
              {muscleGroupLabels[exercise.exerciseMuscleGroup]}
            </span>
          ) : null}
          {exercise.exerciseEquipment ? (
            <span className="inline-flex rounded-full border border-border bg-surface px-3 py-1 text-xs font-medium text-muted">
              {equipmentLabels[exercise.exerciseEquipment]}
            </span>
          ) : null}
        </div>

        <h1 className="text-3xl font-semibold tracking-tight">{exercise.exerciseName}</h1>
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_360px]">
        <div className="space-y-4">
          <div className="space-y-3">
            <h2 className="text-lg font-semibold text-foreground">Опис</h2>
            <div className="rounded-2xl border border-border bg-white px-5 py-5">
              <PlainText
                className="text-sm leading-7 text-muted"
                fallback="Опис поки не додано."
              >
                {exercise.exerciseDescription}
              </PlainText>
            </div>
          </div>

          {exercise.trainerNote ? (
            <div className="rounded-2xl border border-amber-200 bg-amber-50 px-5 py-5">
              <p className="text-xs font-semibold uppercase text-amber-900">Нотатка тренера</p>
              <PlainText className="mt-2 text-sm leading-7 text-amber-950">
                {exercise.trainerNote}
              </PlainText>
            </div>
          ) : null}

          <div className="grid grid-cols-2 gap-3 text-sm md:grid-cols-4">
            <div className="rounded-xl border border-border bg-white px-4 py-4">
              <p className="text-xs text-muted">Підходи</p>
              <p className="mt-1 text-lg font-semibold text-foreground">{exercise.sets}</p>
            </div>
            <div className="rounded-xl border border-border bg-white px-4 py-4">
              <p className="text-xs text-muted">Повторення</p>
              <p className="mt-1 text-lg font-semibold text-foreground">
                {exercise.repetitions}
              </p>
            </div>
            <div className="rounded-xl border border-border bg-white px-4 py-4">
              <p className="text-xs text-muted">Вага</p>
              <p className="mt-1 text-lg font-semibold text-foreground">
                {formatLoad(exercise.loadKg)}
              </p>
            </div>
            <div className="rounded-xl border border-border bg-white px-4 py-4">
              <p className="text-xs text-muted">Відпочинок</p>
              <p className="mt-1 text-lg font-semibold text-foreground">
                {exercise.restSeconds} сек
              </p>
            </div>
          </div>
        </div>

        <div className="space-y-3">
          <h2 className="text-lg font-semibold text-foreground">Медіа</h2>
          <ExerciseDetailMedia mediaAsset={exercise.exerciseMediaAsset} />
        </div>
      </div>
    </section>
  );
}
