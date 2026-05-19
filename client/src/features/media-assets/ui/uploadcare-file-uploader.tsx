"use client";

import { defineLocale } from "@uploadcare/file-uploader";
import ukLocale from "@uploadcare/file-uploader/locales/file-uploader/uk.js";
import { FileUploaderRegular } from "@uploadcare/react-uploader/next";
import "@uploadcare/react-uploader/core.css";
import type { ComponentProps } from "react";

import {
  type AllowedMediaKind,
  defaultAllowedMediaKinds,
  getAcceptedMimeTypes,
} from "@/features/media-assets/model/uploadcare-media";
import { mediaAssetsApi } from "@/lib/api/clients/media-assets-api";
import { uploadcareEnv } from "@/lib/uploadcare/env";

type FileUploaderRegularProps = ComponentProps<typeof FileUploaderRegular>;

let isUkrainianLocaleDefined = false;

interface UploadcareFileUploaderProps
  extends Omit<
    FileUploaderRegularProps,
    | "pubkey"
    | "accept"
    | "sourceList"
    | "filesViewMode"
    | "classNameUploader"
    | "secureUploadsSignatureResolver"
  > {
  allowedKinds?: readonly AllowedMediaKind[];
}

export function UploadcareFileUploader({
  allowedKinds = defaultAllowedMediaKinds,
  ...props
}: UploadcareFileUploaderProps) {
  if (!isUkrainianLocaleDefined) {
    defineLocale("uk", ukLocale);
    isUkrainianLocaleDefined = true;
  }

  return (
    <FileUploaderRegular
      pubkey={uploadcareEnv.publicKey}
      accept={getAcceptedMimeTypes(allowedKinds)}
      sourceList="local, camera, gdrive, facebook"
      filesViewMode="grid"
      classNameUploader="uc-light uc-purple"
      secureUploadsSignatureResolver={mediaAssetsApi.getUploadSignature}
      localeName="uk"
      {...props}
    />
  );
}
