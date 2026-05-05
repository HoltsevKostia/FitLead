export default function AuthLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div className="app-shell">
      <div className="container flex min-h-screen items-center justify-center py-12">
        <div className="card w-full max-w-md p-8">{children}</div>
      </div>
    </div>
  );
}
