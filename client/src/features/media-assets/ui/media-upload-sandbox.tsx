"use client";

import { useState } from "react";

import type { MediaAsset } from "@/entities/media-asset/model/types";
import { RegisteredUploadcareMediaUploader } from "@/features/media-assets/ui/registered-uploadcare-media-uploader";
import { FormAlert } from "@/shared/forms/form-alert";

interface MediaUploadSandboxProps {
  initialMediaAssets: MediaAsset[];
}

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return "Не вдалося зареєструвати файл.";
}

export function MediaUploadSandbox({
  initialMediaAssets,
}: MediaUploadSandboxProps) {
  const [mediaAssets, setMediaAssets] = useState(initialMediaAssets);
  const [selectedMediaAssetId, setSelectedMediaAssetId] = useState<string | null>(
    initialMediaAssets[0]?.id ?? null,
  );
  const [error, setError] = useState<string | null>(null);
  const selectedMediaAsset =
    mediaAssets.find((mediaAsset) => mediaAsset.id === selectedMediaAssetId) ?? null;

  return (
    <section className="space-y-6">
      <div className="space-y-3">
        <p className="text-sm uppercase tracking-[0.2em] text-muted">Media</p>
        <h1 className="text-3xl font-semibold tracking-tight">Тестове завантаження</h1>
      </div>

      <div className="space-y-4">
        <RegisteredUploadcareMediaUploader
          onAssetRegistered={(asset) => {
            setMediaAssets((currentAssets) => {
              if (currentAssets.some((mediaAsset) => mediaAsset.id === asset.id)) {
                return currentAssets;
              }

              return [asset, ...currentAssets];
            });
            setSelectedMediaAssetId(asset.id);
            setError(null);
          }}
          onRegistrationError={(caughtError) => {
            setError(getErrorMessage(caughtError));
          }}
          onUnsupportedFileType={() => {
            setError("Цей тип файлу не підтримується.");
          }}
        />
        <FormAlert message={error} />
      </div>

      <MediaAssetGallery
        mediaAssets={mediaAssets}
        selectedMediaAssetId={selectedMediaAssetId}
        onSelect={setSelectedMediaAssetId}
      />

      {selectedMediaAsset ? (
        <div className="space-y-4">
          <MediaAssetPreview mediaAsset={selectedMediaAsset} />

          <div className="overflow-hidden rounded-2xl border border-border bg-surface">
            <dl className="divide-y divide-border text-sm">
              <MetadataRow label="Id" value={selectedMediaAsset.id} />
              <MetadataRow label="Provider" value={selectedMediaAsset.storageProvider} />
              <MetadataRow label="Storage object id" value={selectedMediaAsset.storageObjectId} />
              <MetadataRow label="Delivery URL" value={selectedMediaAsset.deliveryUrl} />
              <MetadataRow label="File name" value={selectedMediaAsset.fileName ?? "—"} />
              <MetadataRow label="Content type" value={selectedMediaAsset.contentType} />
              <MetadataRow label="Size bytes" value={selectedMediaAsset.sizeBytes.toString()} />
              <MetadataRow label="Kind" value={selectedMediaAsset.kind} />
              <MetadataRow
                label="Duration seconds"
                value={selectedMediaAsset.durationSeconds?.toString() ?? "—"}
              />
              <MetadataRow label="Status" value={selectedMediaAsset.status} />
            </dl>
          </div>
        </div>
      ) : null}
    </section>
  );
}

function MediaAssetGallery({
  mediaAssets,
  selectedMediaAssetId,
  onSelect,
}: {
  mediaAssets: MediaAsset[];
  selectedMediaAssetId: string | null;
  onSelect: (mediaAssetId: string) => void;
}) {
  if (mediaAssets.length === 0) {
    return (
      <p className="rounded-2xl border border-dashed border-border px-4 py-6 text-sm text-muted">
        Завантажених медіафайлів поки немає.
      </p>
    );
  }

  return (
    <div className="space-y-3">
      <h2 className="text-xl font-semibold text-foreground">Мої медіафайли</h2>
      <div className="grid gap-3 md:grid-cols-2">
        {mediaAssets.map((mediaAsset) => {
          const isSelected = mediaAsset.id === selectedMediaAssetId;

          return (
            <button
              key={mediaAsset.id}
              type="button"
              onClick={() => {
                onSelect(mediaAsset.id);
              }}
              className={`min-w-0 rounded-2xl border px-4 py-3 text-left transition ${
                isSelected
                  ? "border-accent bg-accent/5"
                  : "border-border bg-white hover:bg-surface"
              }`}
            >
              <p className="truncate font-medium text-foreground">
                {mediaAsset.fileName ?? mediaAsset.storageObjectId}
              </p>
              <p className="mt-1 text-sm text-muted">
                {mediaAsset.kind} · {mediaAsset.contentType}
              </p>
              <p className="mt-1 text-sm text-muted">
                {mediaAsset.durationSeconds
                  ? `${mediaAsset.durationSeconds} s`
                  : `${mediaAsset.sizeBytes} bytes`}
              </p>
            </button>
          );
        })}
      </div>
    </div>
  );
}

function MediaAssetPreview({ mediaAsset }: { mediaAsset: MediaAsset }) {
  if (mediaAsset.kind === "Image") {
    return (
      <img
        src={mediaAsset.deliveryUrl}
        alt={mediaAsset.fileName ?? "Uploaded image"}
        className="max-h-[32rem] w-full rounded-2xl border border-border object-contain"
      />
    );
  }

  if (mediaAsset.kind === "Video") {
    return (
      <video
        controls
        src={mediaAsset.deliveryUrl}
        className="max-h-[32rem] w-full rounded-2xl border border-border bg-black"
      />
    );
  }

  return <audio controls src={mediaAsset.deliveryUrl} className="w-full" />;
}

function MetadataRow({
  label,
  value,
}: {
  label: string;
  value: string;
}) {
  return (
    <div className="grid gap-1 px-4 py-3 sm:grid-cols-[10rem_minmax(0,1fr)] sm:gap-4">
      <dt className="font-medium text-muted">{label}</dt>
      <dd className="break-all text-foreground">{value}</dd>
    </div>
  );
}
