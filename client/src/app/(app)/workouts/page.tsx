import type { Workout } from "@/entities/workout/model/types";
import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getWorkouts } from "@/features/workouts/server/get-workouts";
import { WorkoutLibraryWorkspace } from "@/features/workouts/ui/workout-library-workspace";

function TrainerOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Тренування</h1>
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-6 py-8">
        <p className="text-lg font-medium text-foreground">
          Цей розділ доступний лише тренеру.
        </p>
        <p className="mt-2 max-w-2xl text-sm leading-7 text-muted">
          Клієнти бачитимуть тренування всередині призначених програм.
        </p>
      </div>
    </section>
  );
}

export default async function WorkoutsPage() {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Trainer") {
    return <TrainerOnlyNotice />;
  }

  let workouts: Workout[] = [];
  let loadError: string | null = null;

  try {
    workouts = await getWorkouts();
  } catch {
    loadError = "Не вдалося завантажити список тренувань. Спробуй оновити сторінку.";
  }

  return <WorkoutLibraryWorkspace workouts={workouts} loadError={loadError} />;
}
