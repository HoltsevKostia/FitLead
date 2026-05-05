import Link from "next/link";

export default function RegisterPage() {
  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <p className="text-sm uppercase tracking-[0.2em] text-muted">
          Авторизація
        </p>
        <h1 className="text-3xl font-semibold tracking-tight">
          Створення акаунта
        </h1>
      </div>

      <form className="space-y-4">
        <input
          className="w-full rounded-2xl border border-border bg-white px-4 py-3 outline-none"
          placeholder="Повне ім’я"
          type="text"
        />
        <input
          className="w-full rounded-2xl border border-border bg-white px-4 py-3 outline-none"
          placeholder="Електронна пошта"
          type="email"
        />
        <input
          className="w-full rounded-2xl border border-border bg-white px-4 py-3 outline-none"
          placeholder="Пароль"
          type="password"
        />
        <button
          type="submit"
          className="w-full rounded-2xl bg-accent px-4 py-3 font-medium text-white transition hover:bg-accent-strong"
        >
          Створити профіль
        </button>
      </form>

      <p className="text-sm text-muted">
        Уже маєш акаунт?{" "}
        <Link href="/login" className="font-medium text-foreground">
          Увійти
        </Link>
      </p>
    </div>
  );
}
