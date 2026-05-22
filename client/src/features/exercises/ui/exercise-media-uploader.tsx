"use client";

import type {
  OutputCollectionState,
  OutputFileEntry,
  UploadCtxProvider,
} from "@uploadcare/file-uploader";
import { forwardRef, useImperativeHandle, useRef, useState } from "react";

import type { MediaAsset } from "@/entities/media-asset/model/types";
import { mapUploadcareFileToUploadedMediaAsset } from "@/features/media-assets/model/uploadcare-media";
import { UploadcareFileUploader } from "@/features/media-assets/ui/uploadcare-file-uploader";
import { mediaAssetsApi } from "@/lib/api/clients/media-assets-api";

const allowedExerciseMediaKinds = ["Image", "Video"] as const;
const uploaderReadyTimeoutMs = 2000;
const uploaderReadyCheckIntervalMs = 50;
const unsupportedMediaMessage = "Додайте фото або відео.";

type UploadResolve = (files: OutputFileEntry<"success">[]) => void;
type UploadReject = (error: Error) => void;
type UploaderApi = NonNullable<
  ReturnType<InstanceType<typeof UploadCtxProvider>["getAPI"]>
>;

interface PendingUpload {
  resolve: UploadResolve;
  reject: UploadReject;
}

interface ExerciseMediaUploaderProps {
  onFileCountChange?: (fileCount: number) => void;
  onUploadFailed?: (message: string) => void;
}

export interface ExerciseMediaUploaderHandle {
  uploadSelectedMedia: () => Promise<MediaAsset | null>;
  clear: () => void;
}

export const ExerciseMediaUploader = forwardRef<
  ExerciseMediaUploaderHandle,
  ExerciseMediaUploaderProps
>(function ExerciseMediaUploader(
  { onFileCountChange, onUploadFailed },
  ref,
) {
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

  async function uploadSelectedMedia(): Promise<MediaAsset | null> {
    const api = await waitForUploaderApi();

    if (!api) {
      throw new Error("Не вдалося завантажити медіа.");
    }

    const state = api.getOutputCollectionState() ?? null;

    if (!state || state.totalCount === 0) {
      return null;
    }

    if (state.status === "failed" || state.failedCount > 0) {
      throw new Error(unsupportedMediaMessage);
    }

    if (state.totalCount > 1) {
      throw new Error("Додайте не більше одного файлу.");
    }

    const files =
      state.status === "success"
        ? [...state.successEntries]
        : await new Promise<OutputFileEntry<"success">[]>((resolve, reject) => {
            pendingUploadRef.current = { resolve, reject };
            api.uploadAll();
          });

    const uploadedMedia = mapUploadcareFileToUploadedMediaAsset(files[0]);

    if (
      !uploadedMedia ||
      !(allowedExerciseMediaKinds as readonly string[]).includes(uploadedMedia.kind)
    ) {
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
    uploadSelectedMedia,
    clear,
  }));

  return (
    <div className="space-y-3">
      <UploadcareFileUploader
        apiRef={uploaderRef}
        allowedKinds={allowedExerciseMediaKinds}
        confirmUpload
        multiple={false}
        multipleMax={1}
        onChange={updateFileCount}
        onCommonUploadSuccess={resolvePendingUpload}
        onCommonUploadFailed={() => {
          rejectPendingUpload("Не вдалося завантажити медіа.");
        }}
      />
      <p className="text-sm text-muted">Обрано файлів: {fileCount}/1</p>
    </div>
  );
});

export type ExerciseMediaUploaderRef = ExerciseMediaUploaderHandle;
