import { notFound } from "next/navigation";

import type { WorkoutDetails } from "@/entities/workout/model/types";
import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getWorkout } from "@/features/workouts/server/get-workout";
import { WorkoutDetailView } from "@/features/workouts/ui/workout-detail-view";
import { isApiError } from "@/lib/api/api-error";

interface WorkoutDetailsPageProps {
  params: Promise<{
    workoutId: string;
  }>;
}

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

async function getVisibleWorkoutOrNotFound(workoutId: string): Promise<WorkoutDetails> {
  try {
    return await getWorkout(workoutId);
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      notFound();
    }

    throw error;
  }
}

export default async function WorkoutDetailsPage({ params }: WorkoutDetailsPageProps) {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Trainer") {
    return <TrainerOnlyNotice />;
  }

  const { workoutId } = await params;
  const workout = await getVisibleWorkoutOrNotFound(workoutId);

  return <WorkoutDetailView workout={workout} />;
}
