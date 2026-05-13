import type { ClientAssignedTrainingProgram } from "@/entities/training-program/model/types";
import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getClientAssignedTrainingPrograms } from "@/features/training-programs/server/get-client-assigned-training-programs";
import { ClientAssignedTrainingProgramList } from "@/features/training-programs/ui/client-assigned-training-program-list";

function ClientOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Мої програми</h1>
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-6 py-8">
        <p className="text-lg font-medium text-foreground">
          Цей розділ доступний лише клієнту.
        </p>
      </div>
    </section>
  );
}

export default async function ClientTrainingProgramsPage() {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Client") {
    return <ClientOnlyNotice />;
  }

  let programs: ClientAssignedTrainingProgram[] = [];
  let loadError: string | null = null;

  try {
    programs = await getClientAssignedTrainingPrograms();
  } catch {
    loadError = "Не вдалося завантажити призначені програми. Спробуйте оновити сторінку.";
  }

  return <ClientAssignedTrainingProgramList programs={programs} loadError={loadError} />;
}
