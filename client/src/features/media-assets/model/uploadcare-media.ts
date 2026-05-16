import type { OutputFileEntry } from "@uploadcare/file-uploader";

import type {
  MediaAssetKind,
  UploadedMediaAssetMetadata,
} from "@/entities/media-asset/model/types";

export const defaultAllowedMediaKinds = ["Image", "Video", "Audio"] as const;

export type AllowedMediaKind = (typeof defaultAllowedMediaKinds)[number];

const acceptByKind: Record<AllowedMediaKind, string> = {
  Image: "image/*",
  Video: "video/*",
  Audio: "audio/*",
};

export function getAcceptedMimeTypes(
  allowedKinds: readonly AllowedMediaKind[],
): string {
  return allowedKinds.map((kind) => acceptByKind[kind]).join(",");
}

export function mapUploadcareFileToUploadedMediaAsset(
  file: OutputFileEntry<"success">,
): UploadedMediaAssetMetadata | null {
  const kind = getMediaAssetKind(file.mimeType);

  if (!kind) {
    return null;
  }

  return {
    storageProvider: "Uploadcare",
    storageObjectId: file.uuid,
    deliveryUrl: file.cdnUrl,
    fileName: file.fileInfo.originalFilename || file.name || null,
    contentType: file.mimeType,
    sizeBytes: file.size,
    kind,
    durationSeconds: getDurationSeconds(file),
  };
}

function getMediaAssetKind(contentType: string): MediaAssetKind | null {
  if (contentType.startsWith("image/")) {
    return "Image";
  }

  if (contentType.startsWith("video/")) {
    return "Video";
  }

  if (contentType.startsWith("audio/")) {
    return "Audio";
  }

  return null;
}

function getDurationSeconds(file: OutputFileEntry<"success">): number | null {
  const duration = file.fileInfo.videoInfo?.duration;

  if (!duration || duration <= 0) {
    return null;
  }

  return Math.ceil(duration);
}
