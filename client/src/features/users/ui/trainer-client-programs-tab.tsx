"use client";

import Link from "next/link";
import { useState } from "react";

import type { TrainerClientProgram } from "@/entities/user/model/types";

interface TrainerClientProgramsTabProps {
  programs: TrainerClientProgram[] | null;
}

function formatDate(value: string): string {
  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(value));
}

function formatOptionalEndDate(value: string | null): string {
  return value ? formatDate(value) : "Безстроково";
}

function getAssignmentStatusLabel(status: string): string {
  if (status === "Active") {
    return "Активна";
  }

  if (status === "Revoked") {
    return "Відкликана";
  }

  if (status === "Expired") {
    return "Завершена";
  }

  return status;
}

function ProgramStats({ program }: { program: TrainerClientProgram }) {
  const pendingLabel = program.status === "Active" ? "Очікує" : "Без запису";
  const stats = [
    {
      label: "Виконано",
      value: program.workoutLogCounts.completed,
      className: "text-emerald-700",
    },
    {
      label: "Пропущено",
      value: program.workoutLogCounts.skipped,
      className: "text-amber-700",
    },
    {
      label: pendingLabel,
      value: program.workoutLogCounts.pending,
      className: "text-sky-700",
    },
  ];

  return (
    <div className="grid grid-cols-3 gap-2">
      {stats.map((stat) => (
        <div
          key={stat.label}
          className="rounded-xl border border-border bg-surface px-3 py-3"
        >
          <p className={`text-xl font-semibold ${stat.className}`}>{stat.value}</p>
          <p className="mt-1 text-xs text-muted">{stat.label}</p>
        </div>
      ))}
    </div>
  );
}

function ProgramCard({ program }: { program: TrainerClientProgram }) {
  return (
    <article className="rounded-2xl border border-border bg-white px-5 py-5">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div className="min-w-0 space-y-2">
          <div className="flex flex-wrap items-center gap-2">
            <span className="rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
              {getAssignmentStatusLabel(program.status)}
            </span>
            <span className="rounded-full border border-border bg-surface px-3 py-1 text-xs font-semibold text-muted">
              {program.totalWorkouts} тренувань
            </span>
          </div>
          <h2 className="break-words text-lg font-semibold text-foreground">
            {program.programTitle}
          </h2>
          <div className="space-y-1 text-sm text-muted">
            <p>Призначено: {formatDate(program.assignedAtUtc)}</p>
            <p>Завершення: {formatOptionalEndDate(program.expiresAtUtc)}</p>
            {program.revokedAtUtc ? (
              <p>Відкликано: {formatDate(program.revokedAtUtc)}</p>
            ) : null}
          </div>
        </div>

        <Link
          href={`/training-programs/${program.programId}`}
          className="w-fit rounded-full bg-accent px-4 py-2 text-sm font-medium text-white transition hover:bg-accent-strong"
        >
          Відкрити деталі
        </Link>
      </div>

      <div className="mt-4">
        <ProgramStats program={program} />
      </div>
    </article>
  );
}

export function TrainerClientProgramsTab({ programs }: TrainerClientProgramsTabProps) {
  const [showInactivePrograms, setShowInactivePrograms] = useState(false);

  if (!programs) {
    return null;
  }

  if (programs.length === 0) {
    return (
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-5 py-6">
        <p className="text-sm text-muted">Програми ще не призначено.</p>
      </div>
    );
  }

  const activePrograms = programs.filter((program) => program.status === "Active");
  const inactivePrograms = programs.filter((program) => program.status !== "Active");
  const visiblePrograms = showInactivePrograms
    ? programs
    : activePrograms;

  return (
    <div className="space-y-4">
      {inactivePrograms.length > 0 ? (
        <div className="flex flex-col gap-3 rounded-2xl border border-border bg-white px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
          <p className="text-sm text-muted">
            Приховано неактивні програми: {inactivePrograms.length}
          </p>
          <button
            type="button"
            onClick={() => setShowInactivePrograms((current) => !current)}
            className="w-fit rounded-full border border-border px-4 py-2 text-sm font-medium text-foreground transition hover:bg-surface-strong"
          >
            {showInactivePrograms ? "Сховати" : "Показати"}
          </button>
        </div>
      ) : null}

      {visiblePrograms.map((program) => (
        <ProgramCard key={program.assignmentId} program={program} />
      ))}
    </div>
  );
}
