"use client";

import type { InputHTMLAttributes } from "react";
import { useState } from "react";

import {
  fieldErrorClassName,
  fieldInputClassName,
  fieldLabelClassName,
} from "@/shared/forms/field-styles";

interface PasswordFieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, "className" | "type"> {
  label: string;
  error?: string;
  placeholder?: string;
  showLabel?: string;
  hideLabel?: string;
}

export function PasswordField({
  id,
  label,
  error,
  showLabel = "Показати",
  hideLabel = "Сховати",
  ...inputProps
}: PasswordFieldProps) {
  const [isVisible, setIsVisible] = useState(false);
  const errorId = error && id ? `${id}-error` : undefined;
  const toggleLabel = isVisible ? hideLabel : showLabel;

  return (
    <div className="space-y-2">
      <label className={fieldLabelClassName} htmlFor={id}>
        {label}
      </label>
      <div className="relative">
        <input
          {...inputProps}
          id={id}
          type={isVisible ? "text" : "password"}
          aria-invalid={error ? "true" : "false"}
          aria-describedby={errorId}
          className={`${fieldInputClassName} pr-28`}
        />
        <button
          type="button"
          onClick={() => setIsVisible((current) => !current)}
          className="absolute right-3 top-1/2 -translate-y-1/2 text-sm font-medium text-muted transition hover:text-foreground focus:outline-none"
          aria-pressed={isVisible}
          aria-label={isVisible ? "Приховати пароль" : "Показати пароль"}
        >
          {toggleLabel}
        </button>
      </div>
      {error ? (
        <p id={errorId} className={fieldErrorClassName}>
          {error}
        </p>
      ) : null}
    </div>
  );
}
