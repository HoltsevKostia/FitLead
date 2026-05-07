import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getTrainerInvitations } from "@/features/invitations/server/get-trainer-invitations";
import { TrainerInvitationsWorkspace } from "@/features/invitations/ui/trainer-invitations-workspace";

function TrainerOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Запрошення</h1>
      <div className="rounded-3xl border border-border bg-surface-strong/50 px-6 py-8">
        <p className="text-lg font-medium text-foreground">
          Цей розділ доступний лише тренеру.
        </p>
        <p className="mt-2 max-w-2xl text-sm leading-7 text-muted">
          Клієнт може приймати запрошення за персональним посиланням, але створювати й
          відкликати їх може тільки тренер.
        </p>
      </div>
    </section>
  );
}

export default async function InvitationsPage() {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Trainer") {
    return <TrainerOnlyNotice />;
  }

  let invitations = [];
  let loadError: string | null = null;

  try {
    invitations = await getTrainerInvitations();
  } catch {
    loadError = "Не вдалося завантажити список запрошень. Спробуй оновити сторінку.";
  }

  return <TrainerInvitationsWorkspace invitations={invitations} loadError={loadError} />;
}
