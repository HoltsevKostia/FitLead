import { notFound } from "next/navigation";

import type { ClientAssignedTrainingProgramDetails } from "@/entities/training-program/model/types";
import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getClientAssignedTrainingProgramDetails } from "@/features/training-programs/server/get-client-assigned-training-program-details";
import { ClientAssignedTrainingProgramDetailView } from "@/features/training-programs/ui/client-assigned-training-program-detail-view";
import { isApiError } from "@/lib/api/api-error";

interface ClientAssignedTrainingProgramDetailsPageProps {
  params: Promise<{
    assignmentId: string;
  }>;
}

function ClientOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Моя програма</h1>
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

export default async function ClientAssignedTrainingProgramDetailsPage({
  params,
}: ClientAssignedTrainingProgramDetailsPageProps) {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Client") {
    return <ClientOnlyNotice />;
  }

  const { assignmentId } = await params;
  const program = await getAssignedProgramOrNotFound(assignmentId);

  return <ClientAssignedTrainingProgramDetailView program={program} />;
}
