import Link from "next/link";

import type { TrainerDashboardSummary } from "@/features/trainer-dashboard/model/types";

interface SummaryCardData {
  label: string;
  value: number;
  description: string;
  href?: string;
}

function SummaryCard({ card }: { card: SummaryCardData }) {
  const content = (
    <>
      <p className="text-sm font-medium text-muted">{card.label}</p>
      <p className="mt-3 text-3xl font-semibold text-foreground">{card.value}</p>
      <p className="mt-2 text-sm leading-5 text-muted">{card.description}</p>
    </>
  );

  if (!card.href) {
    return (
      <div className="min-h-36 rounded-xl border border-border bg-white p-4">
        {content}
      </div>
    );
  }

  return (
    <Link
      href={card.href}
      className="min-h-36 rounded-xl border border-border bg-white p-4 transition hover:border-accent/50 hover:bg-surface"
    >
      {content}
    </Link>
  );
}

export function TrainerDashboardSummary({
  summary,
}: {
  summary: TrainerDashboardSummary;
}) {
  const cards: SummaryCardData[] = [
    {
      label: "Клієнти",
      value: summary.clientCount,
      description: "Усього підключених клієнтів",
      href: "/clients",
    },
    {
      label: "Активні програми",
      value: summary.activeProgramAssignmentCount,
      description: "Чинних призначень програм",
      href: "/clients",
    },
    {
      label: "Сповіщення",
      value: summary.unreadNotificationCount,
      description: "Нових подій очікують перегляду",
    },
    {
      label: "Відеозвіти",
      value: summary.pendingVideoReportCount,
      description: "Очікують відгуку тренера",
    },
  ];

  return (
    <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
      {cards.map((card) => (
        <SummaryCard key={card.label} card={card} />
      ))}
    </div>
  );
}
