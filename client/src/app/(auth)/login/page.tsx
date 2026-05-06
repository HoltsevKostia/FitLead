import Link from "next/link";
import { redirect } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { LoginForm } from "@/features/auth/ui/login-form";

export default async function LoginPage() {
  const currentUser = await getCurrentUser();

  if (currentUser) {
    redirect("/dashboard");
  }

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <p className="text-sm uppercase tracking-[0.2em] text-muted">Авторизація</p>
        <h1 className="text-3xl font-semibold tracking-tight">Вхід</h1>
      </div>

      <LoginForm />

      <p className="text-sm text-muted">
        Ще не маєш акаунта?{" "}
        <Link href="/register" className="font-medium text-foreground">
          Зареєструватися
        </Link>
      </p>
    </div>
  );
}
