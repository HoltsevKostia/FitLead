import type { ReactNode } from "react";

import type { InvitationPreviewStatus } from "@/entities/invitation/model/types";

const statusLabels: Record<InvitationPreviewStatus, string> = {
  Pending: "Активне",
  Accepted: "Використане",
  Expired: "Прострочене",
  Revoked: "Відкликане",
};

const statusClasses: Record<InvitationPreviewStatus, string> = {
  Pending: "border-emerald-200 bg-emerald-50 text-emerald-800",
  Accepted: "border-slate-200 bg-slate-100 text-slate-700",
  Expired: "border-amber-200 bg-amber-50 text-amber-800",
  Revoked: "border-rose-200 bg-rose-50 text-rose-800",
};

interface InvitationPreviewCardProps {
  trainerName: string;
  status: InvitationPreviewStatus;
  expiresAtUtc: string;
  message: string;
  actions?: ReactNode;
}

function formatDate(value: string): string {
  return new Intl.DateTimeFormat("uk-UA", {
    dateStyle: "long",
    timeStyle: "short",
  }).format(new Date(value));
}

export function InvitationPreviewCard({
  trainerName,
  status,
  expiresAtUtc,
  message,
  actions,
}: InvitationPreviewCardProps) {
  return (
    <section className="card overflow-hidden">
      <div className="border-b border-border bg-surface-strong/60 px-6 py-5 sm:px-8">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div className="space-y-2">
            <p className="text-sm uppercase tracking-[0.2em] text-muted">FitLead Invitation</p>
            <h1 className="text-3xl font-semibold tracking-tight sm:text-4xl">
              {trainerName} запрошує тебе приєднатися
            </h1>
          </div>
          <span
            className={`inline-flex rounded-full border px-3 py-1 text-sm font-medium ${statusClasses[status]}`}
          >
            {statusLabels[status]}
          </span>
        </div>
      </div>

      <div className="space-y-6 px-6 py-6 sm:px-8 sm:py-8">
        <div className="grid gap-4 rounded-3xl bg-surface-strong/50 p-5 sm:grid-cols-2">
          <div className="space-y-1">
            <p className="text-sm uppercase tracking-[0.16em] text-muted">Тренер</p>
            <p className="text-lg font-medium text-foreground">{trainerName}</p>
          </div>
          <div className="space-y-1">
            <p className="text-sm uppercase tracking-[0.16em] text-muted">Дійсне до</p>
            <p className="text-lg font-medium text-foreground">{formatDate(expiresAtUtc)}</p>
          </div>
        </div>

        <p className="max-w-2xl text-base leading-7 text-muted">{message}</p>

        {actions ? <div className="pt-2">{actions}</div> : null}
      </div>
    </section>
  );
}
