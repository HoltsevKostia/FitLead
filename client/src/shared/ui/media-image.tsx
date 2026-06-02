import Image from "next/image";

type MediaImageAspectRatio = "video" | "4/5" | "4/3" | "3/2" | "square";
type MediaImageObjectFit = "cover" | "contain";

interface MediaImageProps {
  src: string;
  alt: string;
  aspectRatio?: MediaImageAspectRatio;
  objectFit?: MediaImageObjectFit;
  sizes?: string;
  className?: string;
  imageClassName?: string;
}

const aspectRatioClassNames: Record<MediaImageAspectRatio, string> = {
  video: "aspect-video",
  "4/5": "aspect-[4/5]",
  "4/3": "aspect-[4/3]",
  "3/2": "aspect-[3/2]",
  square: "aspect-square",
};

const objectFitClassNames: Record<MediaImageObjectFit, string> = {
  cover: "object-cover",
  contain: "object-contain",
};

const defaultSizes = "(max-width: 640px) 100vw, (max-width: 1280px) 50vw, 33vw";

export function MediaImage({
  src,
  alt,
  aspectRatio = "4/5",
  objectFit = "cover",
  sizes = defaultSizes,
  className,
  imageClassName,
}: MediaImageProps) {
  return (
    <div
      className={[
        "relative overflow-hidden bg-surface-strong",
        aspectRatioClassNames[aspectRatio],
        className,
      ]
        .filter(Boolean)
        .join(" ")}
    >
      <Image
        src={src}
        alt={alt}
        fill
        sizes={sizes}
        loading="lazy"
        className={[
          objectFitClassNames[objectFit],
          imageClassName,
        ]
          .filter(Boolean)
          .join(" ")}
      />
    </div>
  );
}
