import type {
  CreateInvitationRequest,
  CreateInvitationResult,
  InvitationPreview,
  TrainerInvitation,
} from "@/entities/invitation/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const invitationsApi = {
  getTrainerInvitations(): Promise<TrainerInvitation[]> {
    return apiRequest<TrainerInvitation[]>("/invitations/trainer");
  },

  getPreview(token: string): Promise<InvitationPreview> {
    return apiRequest<InvitationPreview>(`/invitations/${encodeURIComponent(token)}/preview`);
  },

  create(request: CreateInvitationRequest): Promise<CreateInvitationResult> {
    return apiRequest<CreateInvitationResult>("/invitations", {
      method: "POST",
      body: request,
    });
  },

  accept(token: string): Promise<void> {
    return apiRequest<void>(`/invitations/${encodeURIComponent(token)}/accept`, {
      method: "POST",
      responseType: "void",
    });
  },

  revoke(invitationId: string): Promise<void> {
    return apiRequest<void>(`/invitations/${encodeURIComponent(invitationId)}/revoke`, {
      method: "POST",
      responseType: "void",
    });
  },
};
