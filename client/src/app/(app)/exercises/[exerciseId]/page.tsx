import { notFound } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getExercise } from "@/features/exercises/server/get-exercise";
import { ExerciseDetailView } from "@/features/exercises/ui/exercise-detail-view";
import { isApiError } from "@/lib/api/api-error";
import type { Exercise } from "@/entities/exercise/model/types";

interface ExerciseDetailsPageProps {
  params: Promise<{
    exerciseId: string;
  }>;
}

function TrainerOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Вправа</h1>
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-6 py-8">
        <p className="text-lg font-medium text-foreground">
          Цей розділ доступний лише тренеру.
        </p>
        <p className="mt-2 max-w-2xl text-sm leading-7 text-muted">
          Клієнти бачитимуть вправи всередині призначених тренувань.
        </p>
      </div>
    </section>
  );
}

async function getVisibleExerciseOrNotFound(exerciseId: string): Promise<Exercise> {
  try {
    return await getExercise(exerciseId);
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      notFound();
    }

    throw error;
  }
}

export default async function ExerciseDetailsPage({ params }: ExerciseDetailsPageProps) {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Trainer") {
    return <TrainerOnlyNotice />;
  }

  const { exerciseId } = await params;
  const exercise = await getVisibleExerciseOrNotFound(exerciseId);

  return <ExerciseDetailView exercise={exercise} />;
}
