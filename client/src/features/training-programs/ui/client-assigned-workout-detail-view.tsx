import Link from "next/link";

import type { ClientAssignedTrainingProgramWorkout } from "@/entities/training-program/model/types";
import type { WorkoutExerciseDetails } from "@/entities/workout/model/types";
import {
  equipmentLabels,
  muscleGroupLabels,
} from "@/features/exercises/model/exercise-labels";
import { ExerciseMediaPreview } from "@/features/exercises/ui/exercise-media-preview";
import { WorkoutLogSection } from "@/features/workout-logs/ui/workout-log-section";
import { buildClientExercisePath } from "@/features/training-programs/model/client-assigned-program-navigation";
import { PlainText } from "@/shared/ui/plain-text";

interface ClientAssignedWorkoutDetailViewProps {
  assignmentId: string;
  workout: ClientAssignedTrainingProgramWorkout;
  backHref: string;
  currentHref: string;
}

function formatLoad(loadKg: number | null): string {
  if (loadKg === null) {
    return "Без ваги";
  }

  return `${loadKg} кг`;
}

function ExerciseSummaryCard({
  assignmentId,
  programWorkoutId,
  exerciseReturnHref,
  exercise,
}: {
  assignmentId: string;
  programWorkoutId: string;
  exerciseReturnHref: string;
  exercise: WorkoutExerciseDetails;
}) {
  return (
    <article className="rounded-lg border border-border bg-white px-4 py-4 sm:px-5">
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
            <Link
              href={buildClientExercisePath(
                assignmentId,
                programWorkoutId,
                exercise.workoutExerciseId,
                exerciseReturnHref,
              )}
              className="block break-words text-lg font-semibold text-foreground hover:text-accent sm:text-xl"
            >
              {exercise.exerciseName}
            </Link>
            <PlainText
              className="max-w-3xl text-sm leading-6 text-muted"
              fallback="Опис поки не додано."
            >
              {exercise.exerciseDescription}
            </PlainText>
            <ExerciseMediaPreview mediaAsset={exercise.exerciseMediaAsset} />
          </div>

          {exercise.trainerNote ? (
            <div className="rounded-xl border border-amber-200 bg-amber-50 px-4 py-3">
              <p className="text-xs font-semibold uppercase text-amber-900">Нотатка тренера</p>
              <PlainText className="mt-1 text-sm leading-6 text-amber-950">
                {exercise.trainerNote}
              </PlainText>
            </div>
          ) : null}
        </div>

        <div className="grid w-full shrink-0 grid-cols-2 gap-2 text-sm sm:grid-cols-4 lg:w-[360px]">
          <div className="rounded-lg border border-border bg-surface px-3 py-3">
            <p className="text-xs text-muted">Підходи</p>
            <p className="mt-1 font-semibold text-foreground">{exercise.sets}</p>
          </div>
          <div className="rounded-lg border border-border bg-surface px-3 py-3">
            <p className="text-xs text-muted">Повторення</p>
            <p className="mt-1 font-semibold text-foreground">{exercise.repetitions}</p>
          </div>
          <div className="rounded-lg border border-border bg-surface px-3 py-3">
            <p className="text-xs text-muted">Вага</p>
            <p className="mt-1 font-semibold text-foreground">{formatLoad(exercise.loadKg)}</p>
          </div>
          <div className="rounded-lg border border-border bg-surface px-3 py-3">
            <p className="text-xs text-muted">Відпочинок</p>
            <p className="mt-1 font-semibold text-foreground">{exercise.restSeconds} сек</p>
          </div>
        </div>
      </div>
    </article>
  );
}

export function ClientAssignedWorkoutDetailView({
  assignmentId,
  workout,
  backHref,
  currentHref,
}: ClientAssignedWorkoutDetailViewProps) {
  const exercises = [...workout.exercises].sort((first, second) => first.order - second.order);

  return (
    <section className="mx-auto max-w-5xl space-y-6">
      <Link href={backHref} className="text-sm font-medium text-accent hover:text-accent-strong">
        Назад до програми
      </Link>

      <div className="space-y-3">
        <div className="flex flex-wrap items-center gap-2 text-xs text-muted">
          <span className="inline-flex rounded-full border border-border bg-surface px-3 py-1 font-semibold">
            Тиждень {workout.weekNumber}
          </span>
          <span className="inline-flex rounded-full border border-border bg-surface px-3 py-1 font-semibold">
            День {workout.dayNumber}
          </span>
          <span className="inline-flex rounded-full border border-border bg-surface px-3 py-1 font-semibold">
            #{workout.orderInDay}
          </span>
        </div>
        <h1 className="break-words text-3xl font-semibold tracking-tight">{workout.workoutName}</h1>
      </div>

      {exercises.length === 0 ? (
        <div className="rounded-lg border border-dashed border-border px-4 py-6 text-center sm:px-6 sm:py-8">
          <p className="text-lg font-medium text-foreground">У тренуванні ще немає вправ.</p>
        </div>
      ) : (
        <div className="space-y-3">
          <h2 className="text-lg font-semibold text-foreground">Вправи</h2>
          <div className="grid gap-3">
            {exercises.map((exercise) => (
              <ExerciseSummaryCard
                key={exercise.workoutExerciseId}
                assignmentId={assignmentId}
                programWorkoutId={workout.id}
                exerciseReturnHref={currentHref}
                exercise={exercise}
              />
            ))}
          </div>
        </div>
      )}

      <WorkoutLogSection
        assignmentId={assignmentId}
        programWorkoutId={workout.id}
        log={workout.log}
      />
    </section>
  );
}
