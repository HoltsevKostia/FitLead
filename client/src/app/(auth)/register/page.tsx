import Link from "next/link";
import { redirect } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { RegisterForm } from "@/features/auth/ui/register-form";
import { buildAuthHref } from "@/shared/utils/build-auth-href";
import { resolveSafeNextHref } from "@/shared/utils/resolve-safe-next-href";

type AuthSearchParams = {
  next?: string | string[];
};

interface RegisterPageProps {
  searchParams?: Promise<AuthSearchParams>;
}

export default async function RegisterPage({ searchParams }: RegisterPageProps) {
  const [currentUser, resolvedSearchParams] = await Promise.all([
    getCurrentUser(),
    searchParams ?? Promise.resolve<AuthSearchParams>({}),
  ]);

  const nextHref = resolveSafeNextHref(resolvedSearchParams.next, "/dashboard");

  if (currentUser) {
    redirect(nextHref);
  }

  const loginHref = buildAuthHref("/login", nextHref);

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <p className="text-sm uppercase tracking-[0.2em] text-muted">Авторизація</p>
        <h1 className="text-3xl font-semibold tracking-tight">Створення акаунта</h1>
      </div>

      <RegisterForm nextHref={nextHref} />

      <p className="text-sm text-muted">
        Уже маєш акаунт?{" "}
        <Link href={loginHref} className="font-medium text-foreground">
          Увійти
        </Link>
      </p>
    </div>
  );
}
