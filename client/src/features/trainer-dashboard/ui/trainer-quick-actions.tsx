import Link from "next/link";

const quickActions = [
  {
    href: "/invitations#create-invitation",
    label: "Створити запрошення",
    description: "Нове посилання для клієнта",
  },
  {
    href: "/exercises?create=1",
    label: "Додати вправу",
    description: "Нова вправа у власній бібліотеці",
  },
  {
    href: "/workouts?create=1",
    label: "Створити тренування",
    description: "Новий шаблон тренування",
  },
  {
    href: "/training-programs?create=1",
    label: "Створити програму",
    description: "Новий шаблон програми",
  },
] as const;

export function TrainerQuickActions() {
  return (
    <section className="space-y-3">
      <h2 className="text-xl font-semibold text-foreground">Швидкі дії</h2>

      <div className="grid gap-3 sm:grid-cols-2">
        {quickActions.map((action) => (
          <Link
            key={action.href}
            href={action.href}
            className="flex min-h-24 flex-col justify-center rounded-xl border border-border bg-white px-4 py-4 transition hover:border-accent/50 hover:bg-surface"
          >
            <span className="font-semibold text-foreground">{action.label}</span>
            <span className="mt-1 text-sm leading-5 text-muted">
              {action.description}
            </span>
          </Link>
        ))}
      </div>
    </section>
  );
}
