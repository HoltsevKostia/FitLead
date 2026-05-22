import { ExerciseSource, type Exercise } from "@/entities/exercise/model/types";
import type { MediaAssetPreview } from "@/entities/media-asset/model/types";
import {
  equipmentLabels,
  exerciseSourceLabels,
  muscleGroupLabels,
} from "@/features/exercises/model/exercise-labels";
import { fieldInputClassName } from "@/shared/forms/field-styles";
import { PlainText } from "@/shared/ui/plain-text";

export type ExercisePickerSource = "all" | "my" | "platform";

interface ExercisePickerListProps {
  exercises: Exercise[];
  activeSource: ExercisePickerSource;
  search: string;
  selectedExerciseId: string;
  isLoading: boolean;
  isSubmitting: boolean;
  onActiveSourceChange: (source: ExercisePickerSource) => void;
  onSearchChange: (search: string) => void;
  onSelectExercise: (exerciseId: string) => void;
}

const pickerTabs: Array<{ id: ExercisePickerSource; label: string }> = [
  { id: "all", label: "Усі" },
  { id: "my", label: "Мої" },
  { id: "platform", label: "Платформа" },
];

function getFilteredExercises(
  exercises: Exercise[],
  activeSource: ExercisePickerSource,
  search: string,
): Exercise[] {
  const normalizedSearch = search.trim().toLowerCase();

  return exercises.filter((exercise) => {
    const matchesSource =
      activeSource === "all" ||
      (activeSource === "my" && exercise.source === ExerciseSource.Trainer) ||
      (activeSource === "platform" && exercise.source === ExerciseSource.Platform);
    const matchesSearch =
      !normalizedSearch || exercise.name.toLowerCase().includes(normalizedSearch);

    return matchesSource && matchesSearch;
  });
}

function ExerciseMediaIndicator({ mediaAsset }: { mediaAsset: MediaAssetPreview | null }) {
  if (!mediaAsset) {
    return null;
  }

  const labelByType = {
    Image: "Зображення",
    Video: "Відео",
    Audio: "Медіа",
  } satisfies Record<MediaAssetPreview["kind"], string>;

  return (
    <span className="w-fit rounded-full border border-indigo-200 bg-indigo-50 px-2 py-1 text-xs font-medium text-indigo-800">
      {labelByType[mediaAsset.kind]}
    </span>
  );
}

export function ExercisePickerList({
  exercises,
  activeSource,
  search,
  selectedExerciseId,
  isLoading,
  isSubmitting,
  onActiveSourceChange,
  onSearchChange,
  onSelectExercise,
}: ExercisePickerListProps) {
  const visibleExercises = getFilteredExercises(exercises, activeSource, search);

  return (
    <div className="space-y-3">
      <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
        <div className="space-y-1">
          <h2 className="text-lg font-semibold text-foreground">Вибір вправи</h2>
          <p className="text-sm text-muted">
            Оберіть вправу з бібліотеки та задайте параметри виконання.
          </p>
        </div>

        <div className="inline-flex w-fit rounded-2xl border border-border bg-surface p-1">
          {pickerTabs.map((tab) => {
            const isActive = tab.id === activeSource;

            return (
              <button
                key={tab.id}
                type="button"
                aria-pressed={isActive}
                onClick={() => onActiveSourceChange(tab.id)}
                className={`rounded-xl px-3 py-2 text-sm font-medium transition ${
                  isActive
                    ? "bg-accent text-white"
                    : "text-muted hover:bg-surface-strong hover:text-foreground"
                }`}
              >
                {tab.label}
              </button>
            );
          })}
        </div>
      </div>

      <label className="sr-only" htmlFor="exercise-picker-search">
        Пошук вправи
      </label>
      <input
        id="exercise-picker-search"
        value={search}
        onChange={(event) => onSearchChange(event.target.value)}
        disabled={isLoading || isSubmitting}
        placeholder="Пошук вправи"
        className={fieldInputClassName}
      />

      <div className="max-h-[340px] space-y-3 overflow-y-auto rounded-2xl border border-border bg-surface p-3">
        {isLoading ? (
          <p className="px-3 py-4 text-sm text-muted">Завантажуємо вправи...</p>
        ) : null}

        {!isLoading && visibleExercises.length === 0 ? (
          <p className="px-3 py-4 text-sm text-muted">Вправ не знайдено.</p>
        ) : null}

        {visibleExercises.map((exercise) => {
          const isSelected = exercise.id === selectedExerciseId;

          return (
            <button
              key={exercise.id}
              type="button"
              aria-pressed={isSelected}
              onClick={() => onSelectExercise(exercise.id)}
              disabled={isSubmitting}
              className={`w-full rounded-xl border px-4 py-3 text-left transition ${
                isSelected
                  ? "border-accent bg-emerald-50"
                  : "border-border bg-white hover:border-accent"
              }`}
            >
              <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                <div className="min-w-0 space-y-2">
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="text-sm font-semibold text-foreground">
                      {exercise.name}
                    </span>
                    <span className="rounded-full border border-border bg-surface px-2 py-1 text-xs text-muted">
                      {exerciseSourceLabels[exercise.source]}
                    </span>
                    {exercise.muscleGroup ? (
                      <span className="rounded-full border border-border bg-surface px-2 py-1 text-xs text-muted">
                        {muscleGroupLabels[exercise.muscleGroup]}
                      </span>
                    ) : null}
                    {exercise.equipment ? (
                      <span className="rounded-full border border-border bg-surface px-2 py-1 text-xs text-muted">
                        {equipmentLabels[exercise.equipment]}
                      </span>
                    ) : null}
                  </div>
                  <PlainText
                    className="line-clamp-2 text-sm leading-6 text-muted"
                    fallback="Опис поки не додано."
                  >
                    {exercise.description}
                  </PlainText>
                </div>

                <ExerciseMediaIndicator mediaAsset={exercise.mediaAsset} />
              </div>
            </button>
          );
        })}
      </div>
    </div>
  );
}
