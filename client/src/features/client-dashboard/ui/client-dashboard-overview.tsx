import Link from "next/link";

import type {
  ClientDashboard,
  ClientDashboardProgram,
} from "@/features/client-dashboard/model/types";
import { OpenChatButton } from "@/features/chats/ui/open-chat-button";
import { buildClientWorkoutPath } from "@/features/training-programs/model/client-assigned-program-navigation";

interface StatusItemProps {
  label: string;
  value: number;
  tone: "completed" | "skipped" | "pending";
}

const statusToneClassName: Record<StatusItemProps["tone"], string> = {
  completed: "bg-accent/10 text-accent",
  skipped: "bg-red-50 text-red-700",
  pending: "bg-surface-strong text-muted",
};

function StatusItem({ label, value, tone }: StatusItemProps) {
  return (
    <div className={`rounded-lg px-3 py-2 ${statusToneClassName[tone]}`}>
      <p className="text-xs font-medium">{label}</p>
      <p className="mt-1 text-xl font-semibold">{value}</p>
    </div>
  );
}

function TrainerCard({ dashboard }: { dashboard: ClientDashboard }) {
  return (
    <section className="rounded-xl border border-border bg-white p-4 sm:p-5">
      <p className="text-sm font-medium text-muted">Тренер і чат</p>

      {dashboard.trainer ? (
        <div className="mt-3 flex min-w-0 flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <p className="min-w-0 break-words text-lg font-semibold text-foreground">
            {dashboard.trainer.fullName}
          </p>
          <OpenChatButton
            targetId={dashboard.trainer.trainerId}
            targetType="trainer"
            label="Відкрити чат"
          />
        </div>
      ) : (
        <p className="mt-3 text-sm text-muted">Тренера ще не підключено.</p>
      )}
    </section>
  );
}

function ProgramCard({ program }: { program: ClientDashboardProgram }) {
  return (
    <article className="flex min-w-0 flex-col rounded-xl border border-border bg-white p-4 sm:p-5">
      <div className="min-w-0">
        <p className="text-sm font-medium text-muted">Активна програма</p>
        <h2 className="mt-2 break-words text-xl font-semibold text-foreground">
          {program.title}
        </h2>
        <p className="mt-1 text-sm text-muted">
          {program.weeksCount} тиж. · {program.daysPerWeek} дн. на тиждень
        </p>
      </div>

      <div className="mt-4 grid grid-cols-3 gap-2">
        <StatusItem label="Виконано" value={program.completedCount} tone="completed" />
        <StatusItem label="Пропущено" value={program.skippedCount} tone="skipped" />
        <StatusItem label="Очікує" value={program.pendingCount} tone="pending" />
      </div>

      <Link
        href={`/client/training-programs/${program.assignmentId}`}
        className="mt-5 inline-flex min-h-10 w-full items-center justify-center rounded-full border border-border px-4 py-2 text-center text-sm font-medium text-foreground transition hover:bg-surface-strong sm:w-fit"
      >
        Відкрити програму
      </Link>
    </article>
  );
}

function NextWorkoutCard({ program }: { program: ClientDashboardProgram }) {
  const nextWorkout = program.nextWorkout;

  return (
    <article className="flex min-w-0 flex-col rounded-xl border border-border bg-surface p-4 sm:p-5">
      <p className="text-sm font-medium text-muted">Наступне за програмою</p>

      {nextWorkout ? (
        <>
          <h3 className="mt-2 break-words text-xl font-semibold text-foreground">
            {nextWorkout.workoutName}
          </h3>
          <p className="mt-1 text-sm text-muted">
            Тиждень {nextWorkout.weekNumber} · День {nextWorkout.dayNumber}
          </p>

          <Link
            href={buildClientWorkoutPath(
              program.assignmentId,
              nextWorkout.programWorkoutId,
              "/dashboard",
            )}
            className="mt-auto pt-5"
          >
            <span className="inline-flex min-h-10 w-full items-center justify-center rounded-full bg-accent px-4 py-2 text-center text-sm font-semibold text-white transition hover:bg-accent/90 sm:w-fit">
              Відкрити тренування
            </span>
          </Link>
        </>
      ) : (
        <p className="mt-3 text-sm leading-6 text-muted">
          Усі тренування цієї програми вже відмічені.
        </p>
      )}
    </article>
  );
}

export function ClientDashboardOverview({
  dashboard,
}: {
  dashboard: ClientDashboard;
}) {
  return (
    <div className="space-y-5">
      <TrainerCard dashboard={dashboard} />

      {dashboard.activePrograms.length === 0 ? (
        <section className="rounded-xl border border-dashed border-border bg-white p-5">
          <h2 className="text-lg font-semibold text-foreground">Активних програм немає</h2>
          <p className="mt-2 text-sm text-muted">
            Тут з’явиться програма після призначення тренером.
          </p>
          <Link
            href="/client/training-programs"
            className="mt-4 inline-flex min-h-10 w-full items-center justify-center rounded-full border border-border px-4 py-2 text-center text-sm font-medium text-foreground transition hover:bg-surface-strong sm:w-fit"
          >
            Мої програми
          </Link>
        </section>
      ) : (
        <div className="space-y-4">
          {dashboard.activePrograms.map((program) => (
            <section
              key={program.assignmentId}
              className="grid min-w-0 gap-3 lg:grid-cols-2"
            >
              <ProgramCard program={program} />
              <NextWorkoutCard program={program} />
            </section>
          ))}
        </div>
      )}
    </div>
  );
}
