import type { TrainingProgram } from "@/entities/training-program/model/types";
import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getTrainingPrograms } from "@/features/training-programs/server/get-training-programs";
import { TrainingProgramLibraryWorkspace } from "@/features/training-programs/ui/training-program-library-workspace";

function TrainerOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Програми тренувань</h1>
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

interface TrainingProgramsPageProps {
  searchParams: Promise<{
    assignClientId?: string;
    create?: string;
  }>;
}

export default async function TrainingProgramsPage({
  searchParams,
}: TrainingProgramsPageProps) {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Trainer") {
    return <TrainerOnlyNotice />;
  }

  const { assignClientId, create } = await searchParams;
  let programs: TrainingProgram[] = [];
  let loadError: string | null = null;

  try {
    programs = await getTrainingPrograms();
  } catch {
    loadError = "Не вдалося завантажити список програм. Спробуйте оновити сторінку.";
  }

  return (
    <TrainingProgramLibraryWorkspace
      programs={programs}
      loadError={loadError}
      assignClientId={assignClientId}
      initialCreateFormOpen={create === "1"}
    />
  );
}
