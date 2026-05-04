import Link from "next/link";

export default function MarketingLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div className="app-shell">
      <header className="border-b border-border/70">
        <div className="container flex items-center justify-between py-5">
          <Link href="/" className="text-xl font-semibold tracking-tight">
            FitLead
          </Link>
          <nav className="flex items-center gap-6 text-sm">
            <Link href="/about" className="text-muted">
              Про нас
            </Link>
            <Link href="/login" className="text-muted">
              Вхід
            </Link>
            <Link
              href="/register"
              className="rounded-full bg-accent px-4 py-2 font-medium text-white transition hover:bg-accent-strong"
            >
              Почати
            </Link>
          </nav>
        </div>
      </header>
      <main className="flex-1">{children}</main>
    </div>
  );
}
