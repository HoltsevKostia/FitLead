import type { InvitationPreview } from "@/entities/invitation/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export function getInvitationPreview(token: string): Promise<InvitationPreview> {
  return serverApiRequest<InvitationPreview>(
    `/api/invitations/${encodeURIComponent(token)}/preview`,
    { cache: "no-store" },
  );
}
