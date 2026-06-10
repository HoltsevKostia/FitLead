"use client";

import { useState } from "react";

import type { Workout } from "@/entities/workout/model/types";
import { CreateWorkoutForm } from "@/features/workouts/ui/create-workout-form";
import { WorkoutList } from "@/features/workouts/ui/workout-list";

interface WorkoutLibraryWorkspaceProps {
  workouts: Workout[];
  loadError?: string | null;
  initialCreateFormOpen?: boolean;
}

export function WorkoutLibraryWorkspace({
  workouts,
  loadError,
  initialCreateFormOpen = false,
}: WorkoutLibraryWorkspaceProps) {
  const [isCreateFormOpen, setIsCreateFormOpen] = useState(initialCreateFormOpen);

  function handleCreated() {
    setIsCreateFormOpen(false);
  }

  return (
    <section className="space-y-6">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div className="space-y-3">
          <h1 className="text-3xl font-semibold tracking-tight">Тренування</h1>
          <p className="max-w-3xl text-muted">
            Створюйте шаблони тренувань і збирайте їх з вправ, які вже є у вашій бібліотеці.
          </p>
        </div>

        <button
          type="button"
          onClick={() => setIsCreateFormOpen((current) => !current)}
          className="w-fit rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong"
        >
          {isCreateFormOpen ? "Закрити форму" : "Створити тренування"}
        </button>
      </div>

      {isCreateFormOpen ? (
        <CreateWorkoutForm
          onCreated={handleCreated}
          onCancel={() => setIsCreateFormOpen(false)}
        />
      ) : null}

      <WorkoutList workouts={workouts} loadError={loadError} />
    </section>
  );
}
