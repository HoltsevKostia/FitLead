"use client";

import { FileUploaderRegular } from "@uploadcare/react-uploader/next";
import "@uploadcare/react-uploader/core.css";

import type { UploadedMediaAssetMetadata } from "@/entities/media-asset/model/types";
import {
  type AllowedMediaKind,
  defaultAllowedMediaKinds,
  getAcceptedMimeTypes,
  mapUploadcareFileToUploadedMediaAsset,
} from "@/features/media-assets/model/uploadcare-media";
import { mediaAssetsApi } from "@/lib/api/clients/media-assets-api";
import { uploadcareEnv } from "@/lib/uploadcare/env";

interface UploadcareMediaUploaderProps {
  allowedKinds?: readonly AllowedMediaKind[];
  onUploadSuccess: (media: UploadedMediaAssetMetadata) => void;
  onUnsupportedFileType?: () => void;
}

export function UploadcareMediaUploader({
  allowedKinds = defaultAllowedMediaKinds,
  onUploadSuccess,
  onUnsupportedFileType,
}: UploadcareMediaUploaderProps) {
  return (
    <FileUploaderRegular
      pubkey={uploadcareEnv.publicKey}
      accept={getAcceptedMimeTypes(allowedKinds)}
      sourceList="local, camera, gdrive, facebook"
      filesViewMode="grid"
      classNameUploader="uc-light uc-purple"
      secureUploadsSignatureResolver={mediaAssetsApi.getUploadSignature}
      onFileUploadSuccess={(file) => {
        const media = mapUploadcareFileToUploadedMediaAsset(file);

        if (!media || !allowedKinds.includes(media.kind)) {
          onUnsupportedFileType?.();
          return;
        }

        onUploadSuccess(media);
      }}
    />
  );
}
