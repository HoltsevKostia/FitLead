import type { VideoHTMLAttributes } from "react";

type MediaVideoAspectRatio = "video" | "4/5" | "square";
type MediaVideoObjectFit = "contain" | "cover";

interface MediaVideoProps
  extends Omit<VideoHTMLAttributes<HTMLVideoElement>, "className" | "preload" | "controls"> {
  aspectRatio?: MediaVideoAspectRatio;
  objectFit?: MediaVideoObjectFit;
  className?: string;
  videoClassName?: string;
}

const aspectRatioClassNames: Record<MediaVideoAspectRatio, string> = {
  video: "aspect-video",
  "4/5": "aspect-[4/5]",
  square: "aspect-square",
};

const objectFitClassNames: Record<MediaVideoObjectFit, string> = {
  contain: "object-contain",
  cover: "object-cover",
};

export function MediaVideo({
  aspectRatio = "video",
  objectFit = "contain",
  className,
  videoClassName,
  children,
  ...props
}: MediaVideoProps) {
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
      <video
        {...props}
        controls
        playsInline
        preload="metadata"
        className={[
          "absolute inset-0 h-full w-full bg-black",
          objectFitClassNames[objectFit],
          videoClassName,
        ]
          .filter(Boolean)
          .join(" ")}
      >
        {children}
      </video>
    </div>
  );
}
