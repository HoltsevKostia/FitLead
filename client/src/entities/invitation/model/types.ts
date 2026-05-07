export type InvitationPreviewStatus = "Pending" | "Accepted" | "Expired" | "Revoked";
export type InvitationStatus = "Pending" | "Accepted" | "Revoked";
export type InvitationDisplayStatus = InvitationStatus | "Expired";

export interface InvitationTrainerPreview {
  fullName: string;
}

export interface InvitationPreview {
  status: InvitationPreviewStatus;
  isJoinable: boolean;
  expiresAtUtc: string;
  trainer: InvitationTrainerPreview;
}

export interface TrainerInvitation {
  id: string;
  status: InvitationStatus;
  createdAtUtc: string;
  expiresAtUtc: string;
  acceptedAtUtc: string | null;
}

export interface CreateInvitationRequest {
  expiresInDays: 7 | 14;
}

export interface CreateInvitationResult {
  invitationId: string;
  inviteUrl: string;
  expiresAtUtc: string;
}
