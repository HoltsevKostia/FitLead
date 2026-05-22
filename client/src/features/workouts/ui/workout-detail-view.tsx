import Link from "next/link";

import type { WorkoutDetails, WorkoutExerciseDetails } from "@/entities/workout/model/types";
import {
  equipmentLabels,
  muscleGroupLabels,
} from "@/features/exercises/model/exercise-labels";
import { ExerciseMediaPreview } from "@/features/exercises/ui/exercise-media-preview";
import { AddExerciseToWorkoutForm } from "@/features/workouts/ui/add-exercise-to-workout-form";
import { WorkoutExerciseActions } from "@/features/workouts/ui/workout-exercise-actions";
import { PlainText } from "@/shared/ui/plain-text";

interface WorkoutDetailViewProps {
  workout: WorkoutDetails;
}

function formatLoad(loadKg: number | null): string {
  if (loadKg === null) {
    return "Без ваги";
  }

  return `${loadKg} кг`;
}

function WorkoutExerciseCard({
  workoutId,
  exercise,
}: {
  workoutId: string;
  exercise: WorkoutExerciseDetails;
}) {
  return (
    <article className="rounded-2xl border border-border bg-white px-5 py-5">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div className="min-w-0 space-y-3">
          <div className="flex flex-wrap items-center gap-2">
            <span className="inline-flex rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
              {exercise.order}
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

          <div className="space-y-2">
            <h2 className="text-xl font-semibold text-foreground">
              {exercise.exerciseName}
            </h2>
            <PlainText
              className="max-w-3xl text-sm leading-6 text-muted"
              fallback="Опис поки не додано."
            >
              {exercise.exerciseDescription}
            </PlainText>
            <ExerciseMediaPreview mediaUrl={exercise.exerciseMediaUrl} />
          </div>

          {exercise.trainerNote ? (
            <div className="rounded-xl border border-amber-200 bg-amber-50 px-4 py-3">
              <p className="text-xs font-semibold uppercase text-amber-900">
                Нотатка тренера
              </p>
              <PlainText className="mt-1 text-sm leading-6 text-amber-950">
                {exercise.trainerNote}
              </PlainText>
            </div>
          ) : null}
        </div>

        <div className="grid w-full shrink-0 grid-cols-2 gap-2 text-sm sm:grid-cols-4 lg:w-[360px]">
          <div className="rounded-xl border border-border bg-surface px-3 py-3">
            <p className="text-xs text-muted">Підходи</p>
            <p className="mt-1 font-semibold text-foreground">{exercise.sets}</p>
          </div>
          <div className="rounded-xl border border-border bg-surface px-3 py-3">
            <p className="text-xs text-muted">Повторення</p>
            <p className="mt-1 font-semibold text-foreground">{exercise.repetitions}</p>
          </div>
          <div className="rounded-xl border border-border bg-surface px-3 py-3">
            <p className="text-xs text-muted">Вага</p>
            <p className="mt-1 font-semibold text-foreground">{formatLoad(exercise.loadKg)}</p>
          </div>
          <div className="rounded-xl border border-border bg-surface px-3 py-3">
            <p className="text-xs text-muted">Відпочинок</p>
            <p className="mt-1 font-semibold text-foreground">
              {exercise.restSeconds} сек
            </p>
          </div>
        </div>
      </div>

      <div className="mt-4">
        <Link
          href={`/exercises/${exercise.exerciseId}`}
          className="text-sm font-medium text-accent hover:text-accent-strong"
        >
          Переглянути вправу
        </Link>
      </div>

      <WorkoutExerciseActions workoutId={workoutId} exercise={exercise} />
    </article>
  );
}

export function WorkoutDetailView({ workout }: WorkoutDetailViewProps) {
  const exercises = [...workout.exercises].sort((first, second) => first.order - second.order);

  return (
    <section className="space-y-6">
      <Link href="/workouts" className="text-sm font-medium text-accent hover:text-accent-strong">
        Назад до тренувань
      </Link>

      <div className="space-y-3">
        <h1 className="text-3xl font-semibold tracking-tight">{workout.name}</h1>
        <p className="max-w-3xl text-muted">
          Переглядайте вправи у правильній послідовності та параметри виконання для цього
          тренування.
        </p>
      </div>

      <AddExerciseToWorkoutForm workoutId={workout.id} />

      {exercises.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border px-6 py-8 text-center">
          <p className="text-lg font-medium text-foreground">У тренуванні ще немає вправ.</p>
        </div>
      ) : (
        <div className="grid gap-4">
          {exercises.map((exercise) => (
            <WorkoutExerciseCard
              key={exercise.workoutExerciseId}
              workoutId={workout.id}
              exercise={exercise}
            />
          ))}
        </div>
      )}
    </section>
  );
}
