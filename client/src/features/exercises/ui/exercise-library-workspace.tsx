"use client";

import { useMemo, useState } from "react";

import { ExerciseSource, type Exercise } from "@/entities/exercise/model/types";
import { CreateExerciseForm } from "@/features/exercises/ui/create-exercise-form";
import { ExerciseList } from "@/features/exercises/ui/exercise-list";

type ExerciseLibraryTab = "my" | "platform";

interface ExerciseLibraryWorkspaceProps {
  exercises: Exercise[];
  loadError?: string | null;
}

const tabs: Array<{ id: ExerciseLibraryTab; label: string }> = [
  { id: "my", label: "Моя бібліотека" },
  { id: "platform", label: "Бібліотека платформи" },
];

function filterExercises(
  exercises: Exercise[],
  activeTab: ExerciseLibraryTab,
): Exercise[] {
  if (activeTab === "my") {
    return exercises.filter((exercise) => exercise.source === ExerciseSource.Trainer);
  }

  return exercises.filter((exercise) => exercise.source === ExerciseSource.Platform);
}

export function ExerciseLibraryWorkspace({
  exercises,
  loadError,
}: ExerciseLibraryWorkspaceProps) {
  const [activeTab, setActiveTab] = useState<ExerciseLibraryTab>("my");
  const [isCreateFormOpen, setIsCreateFormOpen] = useState(false);
  const visibleExercises = useMemo(
    () => filterExercises(exercises, activeTab),
    [exercises, activeTab],
  );

  function handleCreated() {
    setIsCreateFormOpen(false);
    setActiveTab("my");
  }

  return (
    <section className="space-y-6">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div className="space-y-3">
          <h1 className="text-3xl font-semibold tracking-tight">Вправи</h1>
          <p className="max-w-3xl text-muted">
            Переглядайте власну бібліотеку та готові вправи платформи без додаткових запитів.
          </p>
        </div>

        <button
          type="button"
          onClick={() => setIsCreateFormOpen((current) => !current)}
          className="w-fit rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong"
        >
          {isCreateFormOpen ? "Закрити форму" : "Створити вправу"}
        </button>
      </div>

      {isCreateFormOpen ? (
        <CreateExerciseForm
          onCreated={handleCreated}
          onCancel={() => setIsCreateFormOpen(false)}
        />
      ) : null}

      <div className="inline-flex rounded-2xl border border-border bg-white p-1">
        {tabs.map((tab) => {
          const isActive = tab.id === activeTab;

          return (
            <button
              key={tab.id}
              type="button"
              onClick={() => setActiveTab(tab.id)}
              className={`rounded-xl px-4 py-2 text-sm font-medium transition ${
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

      <ExerciseList
        exercises={visibleExercises}
        loadError={loadError}
        emptyMessage={
          activeTab === "my"
            ? "У вашій бібліотеці ще немає вправ."
            : "Бібліотека платформи поки порожня."
        }
        emptyDescription={
          activeTab === "my"
            ? "Скопіюйте вправу з бібліотеки платформи або створіть власну вправу пізніше."
            : "Після запуску seed вправи платформи з'являться тут."
        }
      />
    </section>
  );
}
