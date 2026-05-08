import type {
  InvitationDisplayStatus,
  TrainerInvitation,
} from "@/entities/invitation/model/types";

const statusPriority: Record<InvitationDisplayStatus, number> = {
  Pending: 0,
  Expired: 1,
  Accepted: 2,
  Revoked: 3,
};

export const invitationDisplayStatusLabels: Record<InvitationDisplayStatus, string> = {
  Pending: "Активне",
  Expired: "Прострочене",
  Accepted: "Прийняте",
  Revoked: "Відкликане",
};

export const invitationDisplayStatusClasses: Record<InvitationDisplayStatus, string> = {
  Pending: "border-emerald-200 bg-emerald-50 text-emerald-800",
  Expired: "border-amber-200 bg-amber-50 text-amber-800",
  Accepted: "border-slate-200 bg-slate-100 text-slate-700",
  Revoked: "border-rose-200 bg-rose-50 text-rose-800",
};

export function getInvitationDisplayStatus(
  invitation: TrainerInvitation,
  now: Date = new Date(),
): InvitationDisplayStatus {
  if (invitation.status === "Pending" && new Date(invitation.expiresAtUtc) <= now) {
    return "Expired";
  }

  return invitation.status;
}

export function isInvitationRevokable(
  invitation: TrainerInvitation,
  now: Date = new Date(),
): boolean {
  return getInvitationDisplayStatus(invitation, now) === "Pending";
}

export function sortTrainerInvitations(
  invitations: ReadonlyArray<TrainerInvitation>,
  now: Date = new Date(),
): TrainerInvitation[] {
  return [...invitations].sort((left, right) => {
    const leftStatus = getInvitationDisplayStatus(left, now);
    const rightStatus = getInvitationDisplayStatus(right, now);

    const priorityDiff = statusPriority[leftStatus] - statusPriority[rightStatus];
    if (priorityDiff !== 0) {
      return priorityDiff;
    }

    return new Date(right.createdAtUtc).getTime() - new Date(left.createdAtUtc).getTime();
  });
}

export function formatInvitationDate(value: string): string {
  return new Intl.DateTimeFormat("uk-UA", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}
