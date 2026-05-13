import { notFound } from "next/navigation";

import type { ClientAssignedTrainingProgramDetails } from "@/entities/training-program/model/types";
import { getCurrentUser } from "@/features/auth/server/get-current-user";
import {
  buildClientWorkoutPath,
  getSafeReturnPath,
} from "@/features/training-programs/model/client-assigned-program-navigation";
import {
  findAssignedWorkout,
  findAssignedWorkoutExercise,
} from "@/features/training-programs/model/client-assigned-program-selectors";
import { getClientAssignedTrainingProgramDetails } from "@/features/training-programs/server/get-client-assigned-training-program-details";
import { ClientAssignedExerciseDetailView } from "@/features/training-programs/ui/client-assigned-exercise-detail-view";
import { isApiError } from "@/lib/api/api-error";

interface ClientAssignedExerciseDetailsPageProps {
  params: Promise<{
    assignmentId: string;
    workoutId: string;
    workoutExerciseId: string;
  }>;
  searchParams: Promise<{
    returnTo?: string;
  }>;
}

function ClientOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Вправа</h1>
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-6 py-8">
        <p className="text-lg font-medium text-foreground">
          Цей розділ доступний лише клієнту.
        </p>
      </div>
    </section>
  );
}

async function getAssignedProgramOrNotFound(
  assignmentId: string,
): Promise<ClientAssignedTrainingProgramDetails> {
  try {
    return await getClientAssignedTrainingProgramDetails(assignmentId);
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      notFound();
    }

    throw error;
  }
}

export default async function ClientAssignedExerciseDetailsPage({
  params,
  searchParams,
}: ClientAssignedExerciseDetailsPageProps) {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Client") {
    return <ClientOnlyNotice />;
  }

  const { assignmentId, workoutId, workoutExerciseId } = await params;
  const { returnTo } = await searchParams;
  const program = await getAssignedProgramOrNotFound(assignmentId);
  const workout = findAssignedWorkout(program, workoutId);

  if (!workout) {
    notFound();
  }

  const exercise = findAssignedWorkoutExercise(workout, workoutExerciseId);

  if (!exercise) {
    notFound();
  }

  const fallback = buildClientWorkoutPath(
    assignmentId,
    workout.workoutId,
    `/client/training-programs/${assignmentId}?week=${workout.weekNumber}`,
  );
  const backHref = getSafeReturnPath(returnTo, fallback);

  return (
    <ClientAssignedExerciseDetailView
      workout={workout}
      exercise={exercise}
      backHref={backHref}
    />
  );
}
