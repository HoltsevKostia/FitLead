import Link from "next/link";

import type { Workout } from "@/entities/workout/model/types";

interface WorkoutListProps {
  workouts: Workout[];
  loadError?: string | null;
}

export function WorkoutList({ workouts, loadError }: WorkoutListProps) {
  return (
    <div className="space-y-6">
      {loadError ? (
        <div className="rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-800">
          {loadError}
        </div>
      ) : null}

      {!loadError && workouts.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border px-6 py-8 text-center">
          <p className="text-lg font-medium text-foreground">Тренувань ще немає.</p>
          <p className="mt-2 text-sm text-muted">
            Створіть перший шаблон тренування і додайте до нього вправи з бібліотеки.
          </p>
        </div>
      ) : null}

      {workouts.length > 0 ? (
        <div className="grid gap-4">
          {workouts.map((workout) => (
            <article
              key={workout.id}
              className="rounded-2xl border border-border bg-white px-5 py-5"
            >
              <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
                <div className="min-w-0">
                  <h2 className="text-xl font-semibold text-foreground">{workout.name}</h2>
                  <p className="mt-2 text-sm text-muted">
                    Шаблон тренування для повторного використання у програмах клієнтів.
                  </p>
                </div>

                <Link
                  href={`/workouts/${workout.id}`}
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
