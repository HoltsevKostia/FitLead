import { ExerciseSource, type Exercise } from "@/entities/exercise/model/types";
import {
  equipmentLabels,
  exerciseSourceDescriptions,
  exerciseSourceLabels,
  muscleGroupLabels,
} from "@/features/exercises/model/exercise-labels";
import { ExerciseActions } from "@/features/exercises/ui/exercise-actions";

interface ExerciseListProps {
  exercises: Exercise[];
  loadError?: string | null;
}

function getSourceBadgeClass(source: ExerciseSource): string {
  if (source === ExerciseSource.Platform) {
    return "border-sky-200 bg-sky-50 text-sky-800";
  }

  return "border-emerald-200 bg-emerald-50 text-emerald-800";
}

export function ExerciseList({ exercises, loadError }: ExerciseListProps) {
  return (
    <section className="space-y-6">
      <div className="space-y-3">
        <h1 className="text-3xl font-semibold tracking-tight">Вправи</h1>
        <p className="max-w-3xl text-muted">
          Бібліотека містить готові вправи платформи та ваші власні вправи.
        </p>
      </div>

      {loadError ? (
        <div className="rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-800">
          {loadError}
        </div>
      ) : null}

      {!loadError && exercises.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border px-6 py-8 text-center">
          <p className="text-lg font-medium text-foreground">Вправ ще немає.</p>
          <p className="mt-2 text-sm text-muted">
            Після додавання власних вправ або seed бібліотеки вони з&apos;являться тут.
          </p>
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
                    <span
                      className={`inline-flex rounded-full border px-3 py-1 text-xs font-semibold ${getSourceBadgeClass(exercise.source)}`}
                      title={exerciseSourceDescriptions[exercise.source]}
                    >
                      {exerciseSourceLabels[exercise.source]}
                    </span>

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
                  </div>
                </div>

                <div className="flex shrink-0 flex-col items-start gap-3 md:items-end">
                  <span className="text-sm text-muted">
                    {exercise.isEditable ? "Можна редагувати" : "Тільки перегляд"}
                  </span>
                  <ExerciseActions exercise={exercise} />
                </div>
              </div>
            </article>
          ))}
        </div>
      ) : null}
    </section>
  );
}
