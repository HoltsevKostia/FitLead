export type ExerciseMediaType = "none" | "image" | "youtube" | "video" | "external";

export interface ExerciseMediaInfo {
  type: ExerciseMediaType;
  url: string | null;
}

const imageExtensions = new Set([".apng", ".avif", ".gif", ".jpg", ".jpeg", ".png", ".webp"]);
const videoExtensions = new Set([".m4v", ".mov", ".mp4", ".ogg", ".ogv", ".webm"]);

function getPathExtension(pathname: string): string | null {
  const extensionStart = pathname.lastIndexOf(".");

  if (extensionStart < 0) {
    return null;
  }

  return pathname.slice(extensionStart).toLowerCase();
}

function isYouTubeUrl(url: URL): boolean {
  const hostname = url.hostname.toLowerCase();

  return (
    hostname === "youtu.be" ||
    hostname === "youtube.com" ||
    hostname.endsWith(".youtube.com")
  );
}

export function getExerciseMediaInfo(mediaUrl: string | null | undefined): ExerciseMediaInfo {
  const trimmedUrl = mediaUrl?.trim();

  if (!trimmedUrl) {
    return { type: "none", url: null };
  }

  try {
    const url = new URL(trimmedUrl);

    if (url.protocol !== "http:" && url.protocol !== "https:") {
      return { type: "external", url: trimmedUrl };
    }

    if (isYouTubeUrl(url)) {
      return { type: "youtube", url: trimmedUrl };
    }

    const extension = getPathExtension(url.pathname);

    if (extension && imageExtensions.has(extension)) {
      return { type: "image", url: trimmedUrl };
    }

    if (extension && videoExtensions.has(extension)) {
      return { type: "video", url: trimmedUrl };
    }

    return { type: "external", url: trimmedUrl };
  } catch {
    return { type: "external", url: trimmedUrl };
  }
}
