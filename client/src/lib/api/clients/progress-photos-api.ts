import type {
  CreateProgressPhotoRequest,
  ProgressPhoto,
} from "@/entities/progress-photo/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const progressPhotosApi = {
  list(): Promise<ProgressPhoto[]> {
    return apiRequest<ProgressPhoto[]>("/client/progress-photos");
  },

  create(request: CreateProgressPhotoRequest): Promise<ProgressPhoto> {
    return apiRequest<ProgressPhoto>("/client/progress-photos", {
      method: "POST",
      body: request,
    });
  },

  delete(photoId: string): Promise<void> {
    return apiRequest<void>(`/client/progress-photos/${photoId}`, {
      method: "DELETE",
      responseType: "void",
    });
  },
};
