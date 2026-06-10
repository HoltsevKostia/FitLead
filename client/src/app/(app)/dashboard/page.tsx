import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getClientDashboard } from "@/features/client-dashboard/server/get-client-dashboard";
import { ClientDashboardOverview } from "@/features/client-dashboard/ui/client-dashboard-overview";
import { getTrainerDashboardSummary } from "@/features/trainer-dashboard/server/get-trainer-dashboard-summary";
import { TrainerQuickActions } from "@/features/trainer-dashboard/ui/trainer-quick-actions";
import { TrainerDashboardSummary } from "@/features/trainer-dashboard/ui/trainer-dashboard-summary";

export default async function DashboardPage() {
  const currentUser = await getCurrentUser();
  const clientDashboard =
    currentUser?.role === "Client" ? await getClientDashboard() : null;
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

      {clientDashboard ? <ClientDashboardOverview dashboard={clientDashboard} /> : null}
    </section>
  );
}
