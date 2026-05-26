import type { ElementType, ReactNode } from "react";

const defaultElement = "p";

type PlainTextElement = "p" | "span" | "div";

interface PlainTextProps {
  children: ReactNode;
  fallback?: ReactNode;
  className?: string;
  as?: PlainTextElement;
}

function hasText(value: ReactNode): boolean {
  if (typeof value === "string") {
    return value.trim().length > 0;
  }

  return value !== null && value !== undefined && value !== false;
}

export function PlainText({
  children,
  fallback,
  className,
  as = defaultElement,
}: PlainTextProps) {
  const Component = as as ElementType;
  const content = hasText(children) ? children : fallback;

  return (
    <Component className={`whitespace-pre-wrap break-words ${className ?? ""}`}>
      {content}
    </Component>
  );
}
