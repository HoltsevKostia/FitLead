import { notFound } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
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

  try {
    const client = await getTrainerClientWorkspace(clientId);

    return <TrainerClientWorkspace client={client} activeTab={tab} />;
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
