import { notFound } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getTrainerClientOverviewSummary } from "@/features/users/server/get-trainer-client-overview-summary";
import { getTrainerClientPrograms } from "@/features/users/server/get-trainer-client-programs";
import { getTrainerClientProgress } from "@/features/users/server/get-trainer-client-progress";
import { getTrainerClientVideoReports } from "@/features/users/server/get-trainer-client-video-reports";
import { getTrainerClientWorkoutLogs } from "@/features/users/server/get-trainer-client-workout-logs";
import { getTrainerClientWorkspace } from "@/features/users/server/get-trainer-client-workspace";
import { TrainerClientWorkspace } from "@/features/users/ui/trainer-client-workspace";
import { isApiError, isUnauthorizedApiError } from "@/lib/api/api-error";

interface ClientWorkspacePageProps {
  params: Promise<{
    clientId: string;
  }>;
  searchParams: Promise<{
    tab?: string;
  }>;
}

const availableTabs = new Set([
  "overview",
  "programs",
  "workout-logs",
  "progress",
  "video-reports",
  "profile",
]);

function getActiveTab(value: string | undefined): string {
  return value && availableTabs.has(value) ? value : "overview";
}

function TrainerOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Клієнт</h1>
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-6 py-8">
        <p className="text-lg font-medium text-foreground">
          Цей розділ доступний лише тренеру.
        </p>
      </div>
    </section>
  );
}

async function getTrainerClientWorkspaceOrNotFound(clientId: string) {
  try {
    return await getTrainerClientWorkspace(clientId);
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      notFound();
    }

    if (isUnauthorizedApiError(error)) {
      throw error;
    }

    throw error;
  }
}

async function getTrainerClientOverviewOrNotFound(clientId: string) {
  try {
    return await getTrainerClientOverviewSummary(clientId);
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      notFound();
    }

    if (isUnauthorizedApiError(error)) {
      throw error;
    }

    throw error;
  }
}

async function getTrainerClientProgramsOrNotFound(clientId: string) {
  try {
    return await getTrainerClientPrograms(clientId);
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      notFound();
    }

    if (isUnauthorizedApiError(error)) {
      throw error;
    }

    throw error;
  }
}

async function getTrainerClientWorkoutLogsOrNotFound(clientId: string) {
  try {
    return await getTrainerClientWorkoutLogs(clientId);
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      notFound();
    }

    if (isUnauthorizedApiError(error)) {
      throw error;
    }

    throw error;
  }
}

async function getTrainerClientProgressOrNotFound(clientId: string) {
  try {
    return await getTrainerClientProgress(clientId);
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      notFound();
    }

    if (isUnauthorizedApiError(error)) {
      throw error;
    }

    throw error;
  }
}

async function getTrainerClientVideoReportsOrNotFound(clientId: string) {
  try {
    return await getTrainerClientVideoReports(clientId);
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      notFound();
    }

    if (isUnauthorizedApiError(error)) {
      throw error;
    }

    throw error;
  }
}

export default async function ClientWorkspacePage({
  params,
  searchParams,
}: ClientWorkspacePageProps) {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Trainer") {
    return <TrainerOnlyNotice />;
  }

  const { clientId } = await params;
  const { tab } = await searchParams;
  const activeTab = getActiveTab(tab);
  const client = await getTrainerClientWorkspaceOrNotFound(clientId);
  const overview =
    activeTab === "overview"
      ? await getTrainerClientOverviewOrNotFound(clientId)
      : null;
  const programs =
    activeTab === "programs"
      ? await getTrainerClientProgramsOrNotFound(clientId)
      : null;
  const workoutLogs =
    activeTab === "workout-logs"
      ? await getTrainerClientWorkoutLogsOrNotFound(clientId)
      : null;
  const progress =
    activeTab === "progress"
      ? await getTrainerClientProgressOrNotFound(clientId)
      : null;
  const videoReports =
    activeTab === "video-reports"
      ? await getTrainerClientVideoReportsOrNotFound(clientId)
      : null;

  return (
    <TrainerClientWorkspace
      client={client}
      activeTab={activeTab}
      overview={overview}
      programs={programs}
      workoutLogs={workoutLogs}
      progress={progress}
      videoReports={videoReports}
    />
  );
}
