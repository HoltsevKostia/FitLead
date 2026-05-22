import Link from "next/link";

import { ExerciseSource, type Exercise } from "@/entities/exercise/model/types";
import {
  equipmentLabels,
  exerciseSourceDescriptions,
  exerciseSourceLabels,
  muscleGroupLabels,
} from "@/features/exercises/model/exercise-labels";
import { ExerciseDetailMedia } from "@/features/exercises/ui/exercise-detail-media";
import { PlainText } from "@/shared/ui/plain-text";

interface ExerciseDetailViewProps {
  exercise: Exercise;
}

function getSourceBadgeClass(source: ExerciseSource): string {
  if (source === ExerciseSource.Platform) {
    return "border-sky-200 bg-sky-50 text-sky-800";
  }

  return "border-emerald-200 bg-emerald-50 text-emerald-800";
}

export function ExerciseDetailView({ exercise }: ExerciseDetailViewProps) {
  return (
    <section className="space-y-6">
      <Link href="/exercises" className="text-sm font-medium text-accent hover:text-accent-strong">
        Назад до вправ
      </Link>

      <div className="space-y-4">
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

        <h1 className="text-3xl font-semibold tracking-tight">{exercise.name}</h1>
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_360px]">
        <div className="space-y-3">
          <h2 className="text-lg font-semibold text-foreground">Опис</h2>
          <div className="rounded-2xl border border-border bg-white px-5 py-5">
            <PlainText
              className="text-sm leading-7 text-muted"
              fallback="Опис поки не додано."
            >
              {exercise.description}
            </PlainText>
          </div>
        </div>

        <div className="space-y-3">
          <h2 className="text-lg font-semibold text-foreground">Медіа</h2>
          <ExerciseDetailMedia mediaAsset={exercise.mediaAsset} />
        </div>
      </div>
    </section>
  );
}
