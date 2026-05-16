import type {
  MediaAsset,
  RegisterMediaAssetRequest,
} from "@/entities/media-asset/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const mediaAssetsApi = {
  getMyAssets(): Promise<MediaAsset[]> {
    return apiRequest<MediaAsset[]>("/api/media/assets");
  },

  register(request: RegisterMediaAssetRequest): Promise<MediaAsset> {
    return apiRequest<MediaAsset>("/api/media/assets", {
      method: "POST",
      body: request,
    });
  },
};
