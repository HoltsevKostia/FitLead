import type { ClientTrainer } from "@/entities/user/model/types";
import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { OpenChatButton } from "@/features/chats/ui/open-chat-button";
import { getTrainerDashboardSummary } from "@/features/trainer-dashboard/server/get-trainer-dashboard-summary";
import { TrainerQuickActions } from "@/features/trainer-dashboard/ui/trainer-quick-actions";
import { TrainerDashboardSummary } from "@/features/trainer-dashboard/ui/trainer-dashboard-summary";
import { getMyTrainer } from "@/features/users/server/get-my-trainer";
import { isApiError } from "@/lib/api/api-error";

async function getTrainerOrNull(): Promise<ClientTrainer | null> {
  try {
    return await getMyTrainer();
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      return null;
    }

    throw error;
  }
}

export default async function DashboardPage() {
  const currentUser = await getCurrentUser();
  const trainer =
    currentUser?.role === "Client" ? await getTrainerOrNull() : null;
  const trainerSummary =
    currentUser?.role === "Trainer" ? await getTrainerDashboardSummary() : null;

  return (
    <section className="space-y-6">
      <div className="space-y-3">
        <p className="text-sm uppercase tracking-[0.2em] text-muted">Огляд</p>
        <h1 className="text-4xl font-semibold tracking-tight">Панель</h1>
      </div>

      {trainerSummary ? <TrainerDashboardSummary summary={trainerSummary} /> : null}
      {trainerSummary ? <TrainerQuickActions /> : null}

      {currentUser?.role === "Client" ? (
        <div className="rounded-2xl border border-border bg-white px-5 py-5">
          <p className="text-lg font-semibold text-foreground">Чат із тренером</p>
          {trainer ? (
            <div className="mt-3 space-y-4">
              <p className="text-sm text-muted">{trainer.fullName}</p>
              <OpenChatButton
                targetId={trainer.trainerId}
                targetType="trainer"
                label="Чат із тренером"
              />
            </div>
          ) : (
            <p className="mt-3 text-sm text-muted">
              Активного тренера ще не підключено.
            </p>
          )}
        </div>
      ) : null}
    </section>
  );
}
