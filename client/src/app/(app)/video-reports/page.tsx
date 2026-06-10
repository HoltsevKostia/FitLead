import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getPendingTrainerVideoReports } from "@/features/video-reports/server/get-pending-trainer-video-reports";
import { PendingVideoReportsList } from "@/features/video-reports/ui/pending-video-reports-list";

function TrainerOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Відеозвіти</h1>
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-6 py-8">
        <p className="text-lg font-medium text-foreground">
          Цей розділ доступний лише тренеру.
        </p>
      </div>
    </section>
  );
}

export default async function VideoReportsPage() {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Trainer") {
    return <TrainerOnlyNotice />;
  }

  const reports = await getPendingTrainerVideoReports();

  return (
    <section className="space-y-6">
      <div className="space-y-2">
        <h1 className="text-3xl font-semibold tracking-tight">Відеозвіти</h1>
        <p className="max-w-3xl text-sm leading-6 text-muted">
          Звіти клієнтів, які очікують на відгук.
        </p>
      </div>

      <PendingVideoReportsList reports={reports} />
    </section>
  );
}
