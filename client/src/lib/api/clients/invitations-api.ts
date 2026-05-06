import type {
  CreateInvitationRequest,
  CreateInvitationResult,
  InvitationPreview,
  TrainerInvitation,
} from "@/entities/invitation/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const invitationsApi = {
  getTrainerInvitations(): Promise<TrainerInvitation[]> {
    return apiRequest<TrainerInvitation[]>("/api/invitations/trainer");
  },

  getPreview(token: string): Promise<InvitationPreview> {
    return apiRequest<InvitationPreview>(`/api/invitations/${encodeURIComponent(token)}/preview`);
  },

  create(request: CreateInvitationRequest): Promise<CreateInvitationResult> {
    return apiRequest<CreateInvitationResult>("/api/invitations", {
      method: "POST",
      body: request,
    });
  },

  accept(token: string): Promise<void> {
    return apiRequest<void>(`/api/invitations/${encodeURIComponent(token)}/accept`, {
      method: "POST",
      responseType: "void",
    });
  },

  revoke(invitationId: string): Promise<void> {
    return apiRequest<void>(`/api/invitations/${encodeURIComponent(invitationId)}/revoke`, {
      method: "POST",
      responseType: "void",
    });
  },
};
