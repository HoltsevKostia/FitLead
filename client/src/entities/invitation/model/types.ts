export type InvitationPreviewStatus = "Pending" | "Accepted" | "Expired" | "Revoked";

export interface InvitationTrainerPreview {
  fullName: string;
}

export interface InvitationPreview {
  status: InvitationPreviewStatus;
  isJoinable: boolean;
  expiresAtUtc: string;
  trainer: InvitationTrainerPreview;
}
