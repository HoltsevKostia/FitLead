import Link from "next/link";

import type { ClientAssignedTrainingProgram } from "@/entities/training-program/model/types";

interface ClientAssignedTrainingProgramListProps {
  programs: ClientAssignedTrainingProgram[];
  loadError?: string | null;
}

function formatDate(value: string | null): string {
  if (!value) {
    return "Безстроково";
  }

  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(value));
}

export function ClientAssignedTrainingProgramList({
  programs,
  loadError,
}: ClientAssignedTrainingProgramListProps) {
  return (
    <section className="space-y-6">
      <div className="space-y-3">
        <h1 className="text-3xl font-semibold tracking-tight">Мої програми</h1>
        <p className="max-w-3xl text-muted">
          Активні тренувальні програми, які призначив тренер.
        </p>
      </div>

      {loadError ? (
        <div className="rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-800">
          {loadError}
        </div>
      ) : null}

      {!loadError && programs.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border px-6 py-8 text-center">
          <p className="text-lg font-medium text-foreground">Активних програм немає.</p>
          <p className="mt-2 text-sm text-muted">
            Програма з&apos;явиться тут після призначення тренером.
          </p>
        </div>
      ) : null}

      {programs.length > 0 ? (
        <div className="grid gap-4">
          {programs.map((program) => (
            <article
              key={program.assignmentId}
              className="rounded-2xl border border-border bg-white px-5 py-5"
            >
              <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
                <div className="min-w-0">
                  <h2 className="text-xl font-semibold text-foreground">{program.title}</h2>
                  <p className="mt-2 text-sm text-muted">
                    Тренер: {program.trainerName} · {program.weeksCount} тиж. ·{" "}
                    {program.daysPerWeek} дн./тиждень
                  </p>
                  <p className="mt-1 text-sm text-muted">
                    Доступ: {formatDate(program.expiresAtUtc)}
                  </p>
                </div>

                <Link
                  href={`/client/training-programs/${program.assignmentId}`}
                  className="w-fit rounded-full border border-border px-4 py-2 text-sm font-medium text-foreground transition hover:bg-surface-strong"
                >
                  Переглянути
                </Link>
              </div>
            </article>
          ))}
        </div>
      ) : null}
    </section>
  );
}
