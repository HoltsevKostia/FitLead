import type {
  ClientProfile,
  UpdateClientProfileRequest,
} from "@/entities/client-profile/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const clientProfileApi = {
  updateProfile(request: UpdateClientProfileRequest): Promise<ClientProfile> {
    return apiRequest<ClientProfile>("/client/profile", {
      method: "PUT",
      body: request,
    });
  },
};
