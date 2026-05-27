"use client";

import { useState } from "react";

import type { TrainingProgram } from "@/entities/training-program/model/types";
import { CreateTrainingProgramForm } from "@/features/training-programs/ui/create-training-program-form";
import { TrainingProgramList } from "@/features/training-programs/ui/training-program-list";

interface TrainingProgramLibraryWorkspaceProps {
  programs: TrainingProgram[];
  loadError?: string | null;
  assignClientId?: string;
}

export function TrainingProgramLibraryWorkspace({
  programs,
  loadError,
  assignClientId,
}: TrainingProgramLibraryWorkspaceProps) {
  const [isCreateFormOpen, setIsCreateFormOpen] = useState(false);

  function handleCreated() {
    setIsCreateFormOpen(false);
  }

  return (
    <section className="space-y-6">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div className="space-y-3">
          <h1 className="text-3xl font-semibold tracking-tight">Програми тренувань</h1>
          <p className="max-w-3xl text-muted">
            Створюйте шаблони програм із тижнями та днями, а далі наповнюйте їх тренуваннями.
          </p>
        </div>

        <button
          type="button"
          onClick={() => setIsCreateFormOpen((current) => !current)}
          className="w-fit rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong"
        >
          {isCreateFormOpen ? "Закрити форму" : "Створити програму"}
        </button>
      </div>

      {isCreateFormOpen ? (
        <CreateTrainingProgramForm
          onCreated={handleCreated}
          onCancel={() => setIsCreateFormOpen(false)}
        />
      ) : null}

      <TrainingProgramList
        programs={programs}
        loadError={loadError}
        assignClientId={assignClientId}
      />
    </section>
  );
}
