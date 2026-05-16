"use client";

import { useState } from "react";

import type { MediaAsset } from "@/entities/media-asset/model/types";
import { RegisteredUploadcareMediaUploader } from "@/features/media-assets/ui/registered-uploadcare-media-uploader";
import { FormAlert } from "@/shared/forms/form-alert";

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return "Не вдалося зареєструвати файл.";
}

export function MediaUploadSandbox() {
  const [mediaAsset, setMediaAsset] = useState<MediaAsset | null>(null);
  const [error, setError] = useState<string | null>(null);

  return (
    <section className="space-y-6">
      <div className="space-y-3">
        <p className="text-sm uppercase tracking-[0.2em] text-muted">Media</p>
        <h1 className="text-3xl font-semibold tracking-tight">Тестове завантаження</h1>
      </div>

      <div className="space-y-4">
        <RegisteredUploadcareMediaUploader
          onAssetRegistered={(asset) => {
            setMediaAsset(asset);
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

      {mediaAsset ? (
        <div className="space-y-4">
          <MediaAssetPreview mediaAsset={mediaAsset} />

          <div className="overflow-hidden rounded-2xl border border-border bg-surface">
            <dl className="divide-y divide-border text-sm">
              <MetadataRow label="Id" value={mediaAsset.id} />
              <MetadataRow label="Provider" value={mediaAsset.storageProvider} />
              <MetadataRow label="Storage object id" value={mediaAsset.storageObjectId} />
              <MetadataRow label="Delivery URL" value={mediaAsset.deliveryUrl} />
              <MetadataRow label="File name" value={mediaAsset.fileName ?? "—"} />
              <MetadataRow label="Content type" value={mediaAsset.contentType} />
              <MetadataRow label="Size bytes" value={mediaAsset.sizeBytes.toString()} />
              <MetadataRow label="Kind" value={mediaAsset.kind} />
              <MetadataRow
                label="Duration seconds"
                value={mediaAsset.durationSeconds?.toString() ?? "—"}
              />
              <MetadataRow label="Status" value={mediaAsset.status} />
            </dl>
          </div>
        </div>
      ) : null}
    </section>
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
