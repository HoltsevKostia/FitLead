import type { Exercise } from "@/entities/exercise/model/types";
import {
  equipmentLabels,
  muscleGroupLabels,
} from "@/features/exercises/model/exercise-labels";
import { CopyExerciseAction } from "@/features/exercises/ui/copy-exercise-action";
import { ExerciseActions } from "@/features/exercises/ui/exercise-actions";
import { ExerciseMediaPreview } from "@/features/exercises/ui/exercise-media-preview";

interface ExerciseListProps {
  exercises: Exercise[];
  copiedPlatformExerciseIds?: ReadonlySet<string>;
  loadError?: string | null;
  emptyMessage?: string;
  emptyDescription?: string;
}

export function ExerciseList({
  exercises,
  copiedPlatformExerciseIds,
  loadError,
  emptyMessage = "Вправ ще немає.",
  emptyDescription = "Після додавання власних вправ або seed бібліотеки вони з'являться тут.",
}: ExerciseListProps) {
  return (
    <div className="space-y-6">
      {loadError ? (
        <div className="rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-800">
          {loadError}
        </div>
      ) : null}

      {!loadError && exercises.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border px-6 py-8 text-center">
          <p className="text-lg font-medium text-foreground">{emptyMessage}</p>
          <p className="mt-2 text-sm text-muted">{emptyDescription}</p>
        </div>
      ) : null}

      {exercises.length > 0 ? (
        <div className="grid gap-4">
          {exercises.map((exercise) => (
            <article
              key={exercise.id}
              className="rounded-2xl border border-border bg-white px-5 py-5"
            >
              <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
                <div className="min-w-0 space-y-3">
                  <div className="flex flex-wrap items-center gap-2">
                    {exercise.muscleGroup ? (
                      <span className="inline-flex rounded-full border border-border bg-surface px-3 py-1 text-xs font-medium text-muted">
                        {muscleGroupLabels[exercise.muscleGroup]}
                      </span>
                    ) : null}

                    {exercise.equipment ? (
                      <span className="inline-flex rounded-full border border-border bg-surface px-3 py-1 text-xs font-medium text-muted">
                        {equipmentLabels[exercise.equipment]}
                      </span>
                    ) : null}
                  </div>

                  <div className="space-y-2">
                    <h2 className="text-xl font-semibold text-foreground">{exercise.name}</h2>
                    <p className="max-w-3xl text-sm leading-6 text-muted">
                      {exercise.description || "Опис поки не додано."}
                    </p>
                    <ExerciseMediaPreview mediaUrl={exercise.mediaUrl} />
                  </div>
                </div>

                <div className="flex shrink-0 flex-col items-start gap-3 md:items-end">
                  <ExerciseActions exercise={exercise} />
                  <CopyExerciseAction
                    exercise={exercise}
                    isAlreadyCopied={copiedPlatformExerciseIds?.has(exercise.id) ?? false}
                  />
                </div>
              </div>
            </article>
          ))}
        </div>
      ) : null}
    </div>
  );
}
