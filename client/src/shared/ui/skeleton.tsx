import { type HTMLAttributes } from "react";

type SkeletonProps = HTMLAttributes<HTMLDivElement>;

function mergeClassName(className?: string): string {
  return ["animate-pulse bg-surface-strong", className].filter(Boolean).join(" ");
}

export function SkeletonBlock({ className, ...props }: SkeletonProps) {
  return <div {...props} aria-hidden="true" className={mergeClassName(className)} />;
}

export function SkeletonText({ className, ...props }: SkeletonProps) {
  return (
    <SkeletonBlock
      className={["h-4 rounded-full", className].filter(Boolean).join(" ")}
      {...props}
    />
  );
}

export function SkeletonCard({ className, ...props }: SkeletonProps) {
  return (
    <div
      {...props}
      aria-hidden="true"
      className={[
        "rounded-2xl border border-border bg-white p-4",
        className,
      ]
        .filter(Boolean)
        .join(" ")}
    />
  );
}
