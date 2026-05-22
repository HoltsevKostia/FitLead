"use client";

import type {
  OutputCollectionState,
  OutputFileEntry,
  UploadCtxProvider,
} from "@uploadcare/file-uploader";
import { forwardRef, useImperativeHandle, useRef, useState } from "react";

import type { UploadedMediaAssetMetadata } from "@/entities/media-asset/model/types";
import { mapUploadcareFileToUploadedMediaAsset } from "@/features/media-assets/model/uploadcare-media";
import { UploadcareFileUploader } from "@/features/media-assets/ui/uploadcare-file-uploader";

const allowedVideoReportKinds = ["Image", "Video"] as const;
const uploaderReadyTimeoutMs = 2000;
const uploaderReadyCheckIntervalMs = 50;

interface VideoReportMediaUploaderProps {
  onFileCountChange: (fileCount: number) => void;
  onUploadFailed: (message: string) => void;
}

export interface VideoReportMediaUploaderHandle {
  uploadSelectedFiles: () => Promise<UploadedMediaAssetMetadata[]>;
  clear: () => void;
}

type UploadResolve = (files: OutputFileEntry<"success">[]) => void;
type UploadReject = (error: Error) => void;
type UploaderApi = NonNullable<
  ReturnType<InstanceType<typeof UploadCtxProvider>["getAPI"]>
>;

interface PendingUpload {
  resolve: UploadResolve;
  reject: UploadReject;
}

export const VideoReportMediaUploader = forwardRef<
  VideoReportMediaUploaderHandle,
  VideoReportMediaUploaderProps
>(function VideoReportMediaUploader(
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

  function getCollectionState(api: UploaderApi): OutputCollectionState | null {
    return api.getOutputCollectionState() ?? null;
  }

  function updateFileCount(state: OutputCollectionState) {
    setFileCount(state.totalCount);
    onFileCountChange(state.totalCount);
  }

  function resolvePendingUpload(state: OutputCollectionState<"success">) {
    if (!pendingUploadRef.current) {
      return;
    }

    pendingUploadRef.current?.resolve([...state.successEntries]);
    pendingUploadRef.current = null;
  }

  function rejectPendingUpload(message: string) {
    if (!pendingUploadRef.current) {
      return;
    }

    pendingUploadRef.current?.reject(new Error(message));
    pendingUploadRef.current = null;
    onUploadFailed(message);
  }

  async function uploadSelectedFiles(): Promise<UploadedMediaAssetMetadata[]> {
    const api = await waitForUploaderApi();

    if (!api) {
      throw new Error("Не вдалося завантажити медіа.");
    }

    const state = getCollectionState(api);

    if (!state || state.totalCount === 0) {
      throw new Error("Додайте фото або відео.");
    }

    if (state.totalCount > 5) {
      throw new Error("Додайте не більше 5 файлів.");
    }

    const files =
      state.status === "success"
        ? [...state.successEntries]
        : await new Promise<OutputFileEntry<"success">[]>((resolve, reject) => {
            pendingUploadRef.current = { resolve, reject };
            api.uploadAll();
          });

    const media = files.map(mapUploadcareFileToUploadedMediaAsset);

    if (media.some((item) => item === null)) {
      throw new Error("Додайте лише фото або відео.");
    }

    return media as UploadedMediaAssetMetadata[];
  }

  function clear() {
    getUploaderApi()?.removeAllFiles();
    setFileCount(0);
    onFileCountChange(0);
  }

  useImperativeHandle(ref, () => ({
    uploadSelectedFiles,
    clear,
  }));

  return (
    <div className="space-y-3">
      <UploadcareFileUploader
        className="video-report-uploadcare-uploader"
        apiRef={uploaderRef}
        allowedKinds={allowedVideoReportKinds}
        confirmUpload
        multiple
        multipleMin={1}
        multipleMax={5}
        onChange={updateFileCount}
        onCommonUploadSuccess={resolvePendingUpload}
        onCommonUploadFailed={() => {
          rejectPendingUpload("Не вдалося завантажити медіа.");
        }}
      />
      <p className="text-sm text-muted">Обрано файлів: {fileCount}/5</p>
    </div>
  );
});

export type VideoReportMediaUploaderRef = VideoReportMediaUploaderHandle;
