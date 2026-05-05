import Link from "next/link";

import { RegisterForm } from "@/features/auth/ui/register-form";

export default function RegisterPage() {
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
