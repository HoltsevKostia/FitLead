import Link from "next/link";
import { redirect } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { LogoutButton } from "@/features/auth/ui/logout-button";

const trainerLinks = [
  { href: "/dashboard", label: "Панель" },
  { href: "/chats", label: "Чати" },
  { href: "/clients", label: "Клієнти" },
  { href: "/exercises", label: "Вправи" },
  { href: "/workouts", label: "Тренування" },
  { href: "/training-programs", label: "Програми" },
  { href: "/invitations", label: "Запрошення" },
];

const clientLinks = [
  { href: "/dashboard", label: "Панель" },
  { href: "/chats", label: "Чати" },
  { href: "/client/training-programs", label: "Мої програми" },
];

export default async function AppLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const currentUser = await getCurrentUser();

  if (!currentUser) {
    redirect("/login");
  }

  const links = currentUser.role === "Trainer" ? trainerLinks : clientLinks;

  return (
    <div className="app-shell">
      <div className="container py-6">
        <div className="grid gap-6 lg:grid-cols-[240px_1fr]">
          <aside className="card p-5">
            <div className="mb-6">
              <p className="text-sm uppercase tracking-[0.2em] text-muted">FitLead</p>
              <h2 className="mt-2 text-2xl font-semibold">Кабінет</h2>
              <p className="mt-2 text-sm text-muted">{currentUser.email}</p>
              <p className="mt-1 text-sm text-muted">
                {currentUser.role === "Trainer" ? "Тренер" : "Клієнт"}
              </p>
            </div>
            <nav className="space-y-2">
              {links.map((link) => (
                <Link
                  key={link.href}
                  href={link.href}
                  className="block rounded-2xl px-4 py-3 text-sm font-medium transition hover:bg-surface-strong"
                >
                  {link.label}
                </Link>
              ))}
            </nav>
            <LogoutButton />
          </aside>
          <main className="card min-h-[70vh] p-6 md:p-8">{children}</main>
        </div>
      </div>
    </div>
  );
}
