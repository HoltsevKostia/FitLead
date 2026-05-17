"use client";

import { useState } from "react";

import type {
  MediaAsset,
  UploadedMediaAssetMetadata,
} from "@/entities/media-asset/model/types";
import type { AllowedMediaKind } from "@/features/media-assets/model/uploadcare-media";
import { UploadcareMediaUploader } from "@/features/media-assets/ui/uploadcare-media-uploader";
import { mediaAssetsApi } from "@/lib/api/clients/media-assets-api";

interface RegisteredUploadcareMediaUploaderProps {
  allowedKinds?: readonly AllowedMediaKind[];
  onAssetRegistered: (mediaAsset: MediaAsset) => void;
  onRegistrationError?: (error: unknown) => void;
  onUnsupportedFileType?: () => void;
}

export function RegisteredUploadcareMediaUploader({
  allowedKinds,
  onAssetRegistered,
  onRegistrationError,
  onUnsupportedFileType,
}: RegisteredUploadcareMediaUploaderProps) {
  const [isRegistering, setIsRegistering] = useState(false);

  async function handleUploadSuccess(media: UploadedMediaAssetMetadata) {
    setIsRegistering(true);

    try {
      const mediaAsset = await mediaAssetsApi.register(media);
      onAssetRegistered(mediaAsset);
    } catch (error) {
      onRegistrationError?.(error);
    } finally {
      setIsRegistering(false);
    }
  }

  return (
    <div className="space-y-2">
      <UploadcareMediaUploader
        allowedKinds={allowedKinds}
        onUploadSuccess={(media) => {
          void handleUploadSuccess(media);
        }}
        onUnsupportedFileType={onUnsupportedFileType}
      />
      {isRegistering ? (
        <p className="text-sm text-muted">Реєструємо файл...</p>
      ) : null}
    </div>
  );
}
