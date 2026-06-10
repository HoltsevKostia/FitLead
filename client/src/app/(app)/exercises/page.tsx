import type { Exercise } from "@/entities/exercise/model/types";
import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getExercises } from "@/features/exercises/server/get-exercises";
import { ExerciseLibraryWorkspace } from "@/features/exercises/ui/exercise-library-workspace";

function TrainerOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Вправи</h1>
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-6 py-8">
        <p className="text-lg font-medium text-foreground">
          Цей розділ доступний лише тренеру.
        </p>
        <p className="mt-2 max-w-2xl text-sm leading-7 text-muted">
          Клієнти бачитимуть вправи всередині призначених тренувань, а бібліотекою вправ керує
          тренер.
        </p>
      </div>
    </section>
  );
}

interface ExercisesPageProps {
  searchParams: Promise<{
    create?: string;
  }>;
}

export default async function ExercisesPage({
  searchParams,
}: ExercisesPageProps) {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Trainer") {
    return <TrainerOnlyNotice />;
  }

  const { create } = await searchParams;
  let exercises: Exercise[] = [];
  let loadError: string | null = null;

  try {
    exercises = await getExercises("all");
  } catch {
    loadError = "Не вдалося завантажити список вправ. Спробуй оновити сторінку.";
  }

  return (
    <ExerciseLibraryWorkspace
      exercises={exercises}
      loadError={loadError}
      initialCreateFormOpen={create === "1"}
    />
  );
}
