"use client";

import { useEffect, useState } from "react";

import type { ProgressPhoto } from "@/entities/progress-photo/model/types";
import { mapProgressPhotoMutationError } from "@/features/client-profile/model/progress-photo-error-mapping";
import { ProgressPhotoForm } from "@/features/client-profile/ui/progress-photo-form";
import { ProgressPhotoCard } from "@/features/client-profile/ui/progress-photo-card";
import { MediaLightbox } from "@/features/media-assets/ui/media-lightbox";
import { progressPhotosApi } from "@/lib/api/clients/progress-photos-api";
import { FormAlert } from "@/shared/forms/form-alert";

function sortPhotos(photos: ProgressPhoto[]): ProgressPhoto[] {
  return [...photos].sort((left, right) => {
    const dateCompare = right.takenAt.localeCompare(left.takenAt);

    if (dateCompare !== 0) {
      return dateCompare;
    }

    return right.createdAtUtc.localeCompare(left.createdAtUtc);
  });
}

const labelText: Record<ProgressPhoto["label"], string> = {
  Front: "Спереду",
  Side: "Збоку",
  Back: "Ззаду",
  Other: "Інше",
};

function getLabelText(label: ProgressPhoto["label"]): string {
  return labelText[label] ?? "Фото";
}

function formatDate(value: string): string {
  const [year, month, day] = value.split("-").map(Number);
  if (!year || !month || !day) {
    return value;
  }

  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(year, month - 1, day));
}

export function ProgressPhotosTab() {
  const [photos, setPhotos] = useState<ProgressPhoto[]>([]);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [deletingPhotoId, setDeletingPhotoId] = useState<string | null>(null);
  const [openedPhoto, setOpenedPhoto] = useState<ProgressPhoto | null>(null);

  useEffect(() => {
    let ignore = false;

    async function loadPhotos() {
      setIsLoading(true);
      setLoadError(null);

      try {
        const loadedPhotos = await progressPhotosApi.list();
        if (!ignore) {
          setPhotos(sortPhotos(loadedPhotos));
        }
      } catch (error) {
        if (!ignore) {
          setLoadError(mapProgressPhotoMutationError(error));
        }
      } finally {
        if (!ignore) {
          setIsLoading(false);
        }
      }
    }

    void loadPhotos();

    return () => {
      ignore = true;
    };
  }, []);

  function handleCreated(photo: ProgressPhoto) {
    setPhotos((currentPhotos) => sortPhotos([photo, ...currentPhotos]));
    setLoadError(null);
    setDeleteError(null);
  }

  async function handleDelete(photo: ProgressPhoto) {
    if (deletingPhotoId) {
      return;
    }

    if (!window.confirm("Видалити фото прогресу?")) {
      return;
    }

    setDeletingPhotoId(photo.id);
    setDeleteError(null);

    try {
      await progressPhotosApi.delete(photo.id);
      setPhotos((currentPhotos) =>
        currentPhotos.filter((currentPhoto) => currentPhoto.id !== photo.id),
      );
    } catch (error) {
      setDeleteError(mapProgressPhotoMutationError(error));
    } finally {
      setDeletingPhotoId(null);
    }
  }

  return (
    <div className="space-y-8">
      <ProgressPhotoForm onCreated={handleCreated} />

      <section className="space-y-4">
        <div>
          <h2 className="text-lg font-semibold text-foreground">Галерея прогресу</h2>
          <p className="mt-1 text-sm text-muted">Фото відсортовані від найновішого до найстарішого.</p>
        </div>

        <FormAlert message={loadError ?? deleteError} />

        {isLoading ? (
          <p className="rounded-2xl border border-border bg-surface-strong/50 px-4 py-5 text-sm text-muted">
            Завантажуємо фото...
          </p>
        ) : loadError ? null : photos.length === 0 ? (
          <p className="rounded-2xl border border-border bg-surface-strong/50 px-4 py-5 text-sm text-muted">
            Фото прогресу ще немає.
          </p>
        ) : (
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {photos.map((photo) => (
              <ProgressPhotoCard
                key={photo.id}
                photo={photo}
                isDeleting={deletingPhotoId === photo.id}
                onOpen={setOpenedPhoto}
                onDelete={(selectedPhoto) => void handleDelete(selectedPhoto)}
              />
            ))}
          </div>
        )}
      </section>

      {openedPhoto ? (
        <MediaLightbox
          asset={openedPhoto.mediaAsset}
          title={formatDate(openedPhoto.takenAt)}
          subtitle={getLabelText(openedPhoto.label)}
          note={openedPhoto.note}
          onClose={() => setOpenedPhoto(null)}
        />
      ) : null}
    </div>
  );
}
