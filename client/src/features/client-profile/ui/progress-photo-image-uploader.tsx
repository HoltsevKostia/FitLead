"use client";

import type {
  OutputCollectionState,
  OutputFileEntry,
  UploadCtxProvider,
} from "@uploadcare/file-uploader";
import { type Ref, useEffect, useImperativeHandle, useRef, useState } from "react";

import type { MediaAsset } from "@/entities/media-asset/model/types";
import { mapUploadcareFileToUploadedMediaAsset } from "@/features/media-assets/model/uploadcare-media";
import { UploadcareFileUploader } from "@/features/media-assets/ui/uploadcare-file-uploader";
import { mediaAssetsApi } from "@/lib/api/clients/media-assets-api";

const allowedProgressPhotoKinds = ["Image"] as const;
const uploaderReadyTimeoutMs = 2000;
const uploaderReadyCheckIntervalMs = 50;
const uploadFailedMessage = "Не вдалося завантажити фото.";
const unsupportedMediaMessage = "Додайте зображення.";

type UploadResolve = (files: OutputFileEntry<"success">[]) => void;
type UploadReject = (error: Error) => void;
type UploaderApi = NonNullable<
  ReturnType<InstanceType<typeof UploadCtxProvider>["getAPI"]>
>;

interface PendingUpload {
  resolve: UploadResolve;
  reject: UploadReject;
}

interface ProgressPhotoImageUploaderProps {
  ref?: Ref<ProgressPhotoImageUploaderHandle>;
  onFileCountChange?: (fileCount: number) => void;
  onUploadFailed?: (message: string) => void;
}

export interface ProgressPhotoImageUploaderHandle {
  uploadSelectedImage: () => Promise<MediaAsset>;
  clear: () => void;
}

export function ProgressPhotoImageUploader({
  ref,
  onFileCountChange,
  onUploadFailed,
}: ProgressPhotoImageUploaderProps) {
  const uploaderRef = useRef<InstanceType<typeof UploadCtxProvider> | null>(null);
  const pendingUploadRef = useRef<PendingUpload | null>(null);
  const [fileCount, setFileCount] = useState(0);

  function getUploaderApi() {
    try {
      return uploaderRef.current?.getAPI() ?? null;
    } catch {
      return null;
    }
  }

  async function waitForUploaderApi(): Promise<UploaderApi | null> {
    const startedAt = Date.now();

    while (Date.now() - startedAt < uploaderReadyTimeoutMs) {
      const api = getUploaderApi();
      if (api) {
        return api;
      }

      await new Promise((resolve) =>
        window.setTimeout(resolve, uploaderReadyCheckIntervalMs),
      );
    }

    return getUploaderApi();
  }

  function updateFileCount(state: OutputCollectionState) {
    setFileCount(state.totalCount);
    onFileCountChange?.(state.totalCount);
  }

  function resolvePendingUpload(state: OutputCollectionState<"success">) {
    if (!pendingUploadRef.current) {
      return;
    }

    pendingUploadRef.current.resolve([...state.successEntries]);
    pendingUploadRef.current = null;
  }

  function rejectPendingUpload(message: string) {
    if (!pendingUploadRef.current) {
      return;
    }

    pendingUploadRef.current.reject(new Error(message));
    pendingUploadRef.current = null;
    onUploadFailed?.(message);
  }

  async function uploadSelectedImage(): Promise<MediaAsset> {
    if (pendingUploadRef.current) {
      throw new Error("Фото вже завантажується.");
    }

    const api = await waitForUploaderApi();

    if (!api) {
      throw new Error(uploadFailedMessage);
    }

    const state = api.getOutputCollectionState() ?? null;

    if (!state || state.totalCount === 0) {
      throw new Error(unsupportedMediaMessage);
    }

    if (state.status === "failed" || state.failedCount > 0) {
      throw new Error(unsupportedMediaMessage);
    }

    if (state.totalCount > 1) {
      throw new Error("Додайте не більше одного фото.");
    }

    const files =
      state.status === "success"
        ? [...state.successEntries]
        : await new Promise<OutputFileEntry<"success">[]>((resolve, reject) => {
            pendingUploadRef.current = { resolve, reject };
            api.uploadAll();
          });

    const file = files[0];
    if (!file) {
      throw new Error(uploadFailedMessage);
    }

    const uploadedMedia = mapUploadcareFileToUploadedMediaAsset(file);

    if (!uploadedMedia || uploadedMedia.kind !== "Image") {
      throw new Error(unsupportedMediaMessage);
    }

    return mediaAssetsApi.register(uploadedMedia);
  }

  function clear() {
    getUploaderApi()?.removeAllFiles();
    setFileCount(0);
    onFileCountChange?.(0);
  }

  useImperativeHandle(ref, () => ({
    uploadSelectedImage,
    clear,
  }));

  useEffect(() => {
    return () => {
      if (pendingUploadRef.current) {
        pendingUploadRef.current.reject(new Error(uploadFailedMessage));
        pendingUploadRef.current = null;
      }
    };
  }, []);

  return (
    <div className="space-y-3">
      <UploadcareFileUploader
        apiRef={uploaderRef}
        allowedKinds={allowedProgressPhotoKinds}
        confirmUpload
        multiple={false}
        multipleMax={1}
        onChange={updateFileCount}
        onCommonUploadSuccess={resolvePendingUpload}
        onCommonUploadFailed={() => {
          rejectPendingUpload(uploadFailedMessage);
        }}
      />
      <p className="text-sm text-muted">Обрано файлів: {fileCount}/1</p>
    </div>
  );
}

export type ProgressPhotoImageUploaderRef = ProgressPhotoImageUploaderHandle;
