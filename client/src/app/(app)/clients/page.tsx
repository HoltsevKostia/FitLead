import type { TrainerClientOverview } from "@/entities/user/model/types";
import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getTrainerClientsOverview } from "@/features/users/server/get-trainer-clients-overview";
import { TrainerClientsWorkspace } from "@/features/users/ui/trainer-clients-workspace";

function TrainerOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Клієнти</h1>
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-6 py-8">
        <p className="text-lg font-medium text-foreground">
          Цей розділ доступний лише тренеру.
        </p>
        <p className="mt-2 max-w-2xl text-sm leading-7 text-muted">
          Тут тренер бачить своїх клієнтів та їхні активні програми.
        </p>
      </div>
    </section>
  );
}

export default async function ClientsPage() {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Trainer") {
    return <TrainerOnlyNotice />;
  }

  let clients: TrainerClientOverview[] = [];
  let loadError: string | null = null;

  try {
    clients = await getTrainerClientsOverview();
  } catch {
    loadError = "Не вдалося завантажити список клієнтів. Спробуйте оновити сторінку.";
  }

  return <TrainerClientsWorkspace clients={clients} loadError={loadError} />;
}
