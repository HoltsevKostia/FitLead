import type { ClientProfile } from "@/entities/client-profile/model/types";
import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { getClientProfile } from "@/features/client-profile/server/get-client-profile";
import { ClientProfileWorkspace } from "@/features/client-profile/ui/client-profile-workspace";

function ClientOnlyNotice() {
  return (
    <section className="space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">Профіль клієнта</h1>
      <div className="rounded-2xl border border-border bg-surface-strong/50 px-6 py-8">
        <p className="text-lg font-medium text-foreground">
          Цей розділ доступний лише клієнту.
        </p>
      </div>
    </section>
  );
}

export default async function ClientProfilePage() {
  const currentUser = await getCurrentUser();

  if (!currentUser || currentUser.role !== "Client") {
    return <ClientOnlyNotice />;
  }

  let profile: ClientProfile | null = null;
  let loadError: string | null = null;

  try {
    profile = await getClientProfile();
  } catch {
    loadError = "Не вдалося завантажити профіль. Спробуйте оновити сторінку.";
  }

  const profileVersion = profile?.updatedAtUtc ?? profile?.createdAtUtc ?? "empty";

  return (
    <ClientProfileWorkspace
      key={`${profile?.clientId ?? "unknown"}-${profileVersion}`}
      profile={profile}
      loadError={loadError}
    />
  );
}
