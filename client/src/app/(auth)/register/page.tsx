import Link from "next/link";
import { redirect } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { RegisterForm } from "@/features/auth/ui/register-form";

export default async function RegisterPage() {
  const currentUser = await getCurrentUser();

  if (currentUser) {
    redirect("/dashboard");
  }

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <p className="text-sm uppercase tracking-[0.2em] text-muted">Авторизація</p>
        <h1 className="text-3xl font-semibold tracking-tight">Створення акаунта</h1>
      </div>

      <RegisterForm />

      <p className="text-sm text-muted">
        Уже маєш акаунт?{" "}
        <Link href="/login" className="font-medium text-foreground">
          Увійти
        </Link>
      </p>
    </div>
  );
}
