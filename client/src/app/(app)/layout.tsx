import { redirect } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { LogoutButton } from "@/features/auth/ui/logout-button";
import { AppNavigation } from "@/features/navigation/ui/app-navigation";
import { NotificationBell } from "@/features/notifications/ui/notification-bell";

export default async function AppLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const currentUser = await getCurrentUser();

  if (!currentUser) {
    redirect("/login");
  }

  return (
    <div className="app-shell">
      <div className="container py-6">
        <div className="grid min-w-0 gap-6 lg:grid-cols-[240px_minmax(0,1fr)]">
          <aside className="card flex min-w-0 flex-col p-4 sm:p-5">
            <div className="mb-4 lg:mb-6">
              <div className="flex items-start justify-between gap-3">
                <p className="text-sm uppercase tracking-[0.2em] text-muted">FitLead</p>
                <NotificationBell />
              </div>
              <h2 className="mt-2 text-2xl font-semibold">Кабінет</h2>
              <p className="mt-2 break-words text-sm text-muted">{currentUser.email}</p>
              <p className="mt-1 text-sm text-muted">
                {currentUser.role === "Trainer" ? "Тренер" : "Клієнт"}
              </p>
            </div>
            <AppNavigation role={currentUser.role} />
            <div className="mt-4 lg:mt-auto lg:pt-2">
              <LogoutButton />
            </div>
          </aside>
          <main className="card min-w-0 overflow-hidden p-4 sm:p-6 md:min-h-[70vh] md:p-8">
            {children}
          </main>
        </div>
      </div>
    </div>
  );
}
