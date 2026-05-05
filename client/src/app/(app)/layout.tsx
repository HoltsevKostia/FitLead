import Link from "next/link";

const links = [
  { href: "/dashboard", label: "Панель" },
  { href: "/exercises", label: "Вправи" },
  { href: "/workouts", label: "Тренування" },
  { href: "/training-programs", label: "Програми" },
  { href: "/invitations", label: "Запрошення" },
];

export default function AppLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div className="app-shell">
      <div className="container py-6">
        <div className="grid gap-6 lg:grid-cols-[240px_1fr]">
          <aside className="card p-5">
            <div className="mb-6">
              <p className="text-sm uppercase tracking-[0.2em] text-muted">
                FitLead
              </p>
              <h2 className="mt-2 text-2xl font-semibold">Кабінет тренера</h2>
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
          </aside>
          <main className="card min-h-[70vh] p-6 md:p-8">{children}</main>
        </div>
      </div>
    </div>
  );
}
