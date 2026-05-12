import { notFound } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getTrainingProgram } from "@/features/training-programs/server/get-training-program";
import { getTrainingProgramWorkouts } from "@/features/training-programs/server/get-training-program-workouts";
import { TrainingProgramDetailView } from "@/features/training-programs/ui/training-program-detail-view";
import { getTrainerClients } from "@/features/users/server/get-trainer-clients";
import { getWorkouts } from "@/features/workouts/server/get-workouts";
import { isApiError } from "@/lib/api/api-error";
import type {
  TrainingProgram,
  TrainingProgramWorkout,
} from "@/entities/training-program/model/types";
import type { TrainerClient } from "@/entities/user/model/types";
import type { Workout } from "@/entities/workout/model/types";

interface TrainingProgramDetailsPageProps {
  params: Promise<{
    programId: string;
  }>;
}

function TrainerOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Програма тренувань</h1>
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-6 py-8">
        <p className="text-lg font-medium text-foreground">
          Цей розділ доступний лише тренеру.
        </p>
        <p className="mt-2 max-w-2xl text-sm leading-7 text-muted">
          Клієнти бачитимуть програми після призначення тренером.
        </p>
      </div>
    </section>
  );
}

async function getProgramDetailsOrNotFound(programId: string): Promise<{
  program: TrainingProgram;
  workouts: TrainingProgramWorkout[];
  availableWorkouts: Workout[];
  clients: TrainerClient[];
}> {
  try {
    const [program, workouts, availableWorkouts, clients] = await Promise.all([
      getTrainingProgram(programId),
      getTrainingProgramWorkouts(programId),
      getWorkouts(),
      getTrainerClients(),
    ]);

    return { program, workouts, availableWorkouts, clients };
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      notFound();
    }

    throw error;
  }
}

export default async function TrainingProgramDetailsPage({
  params,
}: TrainingProgramDetailsPageProps) {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Trainer") {
    return <TrainerOnlyNotice />;
  }

  const { programId } = await params;
  const { program, workouts, availableWorkouts, clients } =
    await getProgramDetailsOrNotFound(programId);

  return (
    <TrainingProgramDetailView
      program={program}
      workouts={workouts}
      availableWorkouts={availableWorkouts}
      clients={clients}
    />
  );
}
