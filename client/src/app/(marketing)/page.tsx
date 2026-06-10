import { redirect } from "next/navigation";
import Link from "next/link";

import { getCurrentUser } from "@/features/auth/server/get-current-user";

const highlights = [
  "Бібліотека вправ, яку веде тренер",
  "Конструктор тренувань із повторним використанням блоків",
  "Супровід клієнта через чат і відеозвіти",
];

export default async function LandingPage() {
  const currentUser = await getCurrentUser();

  if (currentUser) {
    redirect("/dashboard");
  }

  return (
    <div className="container py-12 md:py-20">
      <section className="grid gap-10 md:grid-cols-[1.2fr_0.8fr] md:items-center">
        <div className="space-y-6">
          <div className="space-y-4">
            <h1 className="max-w-3xl text-5xl font-semibold tracking-tight md:text-7xl">
              Робочий простір тренера без хаосу в таблицях і нотатках.
            </h1>
            <p className="max-w-2xl text-lg leading-8 text-muted md:text-xl">
              FitLead дає тренеру єдине місце для керування вправами,
              тренуваннями, програмами та спілкуванням з кілєнтами.
            </p>
          </div>
          <div className="flex flex-col gap-3 sm:flex-row">
            <Link
              href="/register"
              className="rounded-full bg-accent px-6 py-3 text-center font-medium text-white transition hover:bg-accent-strong"
            >
              Створити акаунт
            </Link>
            <Link
              href="/about"
              className="rounded-full border border-border px-6 py-3 text-center font-medium transition hover:bg-surface"
            >
              Дізнатися більше
            </Link>
          </div>
        </div>

        <aside className="card p-6 md:p-8">
          <p className="text-sm uppercase tracking-[0.2em] text-muted">
            Наш фокус
          </p>
          <ul className="mt-6 space-y-4">
            {highlights.map((item) => (
              <li
                key={item}
                className="rounded-2xl bg-surface-strong px-4 py-4 text-base font-medium"
              >
                {item}
              </li>
            ))}
          </ul>
        </aside>
      </section>
    </div>
  );
}
