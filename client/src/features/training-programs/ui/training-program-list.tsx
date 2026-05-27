import Link from "next/link";

import type { TrainingProgram } from "@/entities/training-program/model/types";

interface TrainingProgramListProps {
  programs: TrainingProgram[];
  loadError?: string | null;
  assignClientId?: string;
}

function buildProgramHref(programId: string, assignClientId?: string): string {
  if (!assignClientId) {
    return `/training-programs/${programId}`;
  }

  return `/training-programs/${programId}?assignClientId=${encodeURIComponent(assignClientId)}`;
}

export function TrainingProgramList({
  programs,
  loadError,
  assignClientId,
}: TrainingProgramListProps) {
  return (
    <div className="space-y-6">
      {loadError ? (
        <div className="rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-800">
          {loadError}
        </div>
      ) : null}

      {!loadError && programs.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border px-6 py-8 text-center">
          <p className="text-lg font-medium text-foreground">Програм ще немає.</p>
          <p className="mt-2 text-sm text-muted">
            Створіть перший шаблон програми з кількістю тижнів і днів у тижні.
          </p>
        </div>
      ) : null}

      {programs.length > 0 ? (
        <div className="grid gap-4">
          {programs.map((program) => (
            <article
              key={program.id}
              className="rounded-2xl border border-border bg-white px-5 py-5"
            >
              <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
                <div className="min-w-0">
                  <h2 className="text-xl font-semibold text-foreground">{program.title}</h2>
                  <p className="mt-2 text-sm text-muted">
                    {program.weeksCount} тиж. · {program.daysPerWeek} дн./тиждень
                  </p>
                </div>

                <Link
                  href={buildProgramHref(program.id, assignClientId)}
                  className="w-fit rounded-full border border-border px-4 py-2 text-sm font-medium text-foreground transition hover:bg-surface-strong"
                >
                  Переглянути
                </Link>
              </div>
            </article>
          ))}
        </div>
      ) : null}
    </div>
  );
}
