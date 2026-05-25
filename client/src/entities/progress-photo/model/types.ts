import type { MediaAssetPreview } from "@/entities/media-asset/model/types";

export type ProgressPhotoLabel = "Front" | "Side" | "Back" | "Other";

export interface ProgressPhoto {
  id: string;
  clientId: string;
  mediaAssetId: string;
  mediaAsset: MediaAssetPreview;
  takenAt: string;
  label: ProgressPhotoLabel;
  note: string | null;
  createdAtUtc: string;
}

export interface CreateProgressPhotoRequest {
  mediaAssetId: string;
  takenAt: string;
  label: ProgressPhotoLabel;
  note: string | null;
}
