export type MediaStorageProvider = "Uploadcare" | "S3" | "LocalDev";
export type MediaAssetKind = "Image" | "Video" | "Audio";

export interface UploadedMediaAssetMetadata {
  storageProvider: MediaStorageProvider;
  storageObjectId: string;
  deliveryUrl: string;
  fileName: string | null;
  contentType: string;
  sizeBytes: number;
  kind: MediaAssetKind;
  durationSeconds: number | null;
}

export interface MediaAsset {
  id: string;
  storageProvider: MediaStorageProvider;
  storageObjectId: string;
  deliveryUrl: string;
  fileName: string | null;
  contentType: string;
  sizeBytes: number;
  kind: MediaAssetKind;
  durationSeconds: number | null;
  status: string;
  createdAtUtc: string;
}

export type RegisterMediaAssetRequest = UploadedMediaAssetMetadata;
