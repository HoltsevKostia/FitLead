import type { InvitationPreview } from "@/entities/invitation/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const invitationsApi = {
  getPreview(token: string): Promise<InvitationPreview> {
    return apiRequest<InvitationPreview>(`/api/invitations/${encodeURIComponent(token)}/preview`);
  },

  accept(token: string): Promise<void> {
    return apiRequest<void>(`/api/invitations/${encodeURIComponent(token)}/accept`, {
      method: "POST",
      responseType: "void",
    });
  },
};
