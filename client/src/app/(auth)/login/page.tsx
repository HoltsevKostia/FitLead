import Link from "next/link";
import { redirect } from "next/navigation";

import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { LoginForm } from "@/features/auth/ui/login-form";
import { buildAuthHref } from "@/shared/utils/build-auth-href";
import { resolveSafeNextHref } from "@/shared/utils/resolve-safe-next-href";

type AuthSearchParams = {
  next?: string | string[];
};

interface LoginPageProps {
  searchParams?: Promise<AuthSearchParams>;
}

export default async function LoginPage({ searchParams }: LoginPageProps) {
  const [currentUser, resolvedSearchParams] = await Promise.all([
    getCurrentUser(),
    searchParams ?? Promise.resolve<AuthSearchParams>({}),
  ]);

  const nextHref = resolveSafeNextHref(resolvedSearchParams.next, "/dashboard");

  if (currentUser) {
    redirect(nextHref);
  }

  const registerHref = buildAuthHref("/register", nextHref);

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <p className="text-sm uppercase tracking-[0.2em] text-muted">Авторизація</p>
        <h1 className="text-3xl font-semibold tracking-tight">Вхід</h1>
      </div>

      <LoginForm nextHref={nextHref} />

      <p className="text-sm text-muted">
        Ще не маєш акаунта?{" "}
        <Link href={registerHref} className="font-medium text-foreground">
          Зареєструватися
        </Link>
      </p>
    </div>
  );
}
