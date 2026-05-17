import type { MediaAsset } from "@/entities/media-asset/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export function getMyMediaAssets(): Promise<MediaAsset[]> {
  return serverApiRequest<MediaAsset[]>("/api/media/assets");
}
