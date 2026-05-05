import type { InputHTMLAttributes } from "react";

import {
  fieldErrorClassName,
  fieldInputClassName,
  fieldLabelClassName,
} from "@/shared/forms/field-styles";

interface TextFieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, "className"> {
  label: string;
  error?: string;
}

export function TextField({ id, label, error, ...inputProps }: TextFieldProps) {
  const errorId = error && id ? `${id}-error` : undefined;

  return (
    <div className="space-y-2">
      <label className={fieldLabelClassName} htmlFor={id}>
        {label}
      </label>
      <input
        {...inputProps}
        id={id}
        aria-invalid={error ? "true" : "false"}
        aria-describedby={errorId}
        className={fieldInputClassName}
      />
      {error ? (
        <p id={errorId} className={fieldErrorClassName}>
          {error}
        </p>
      ) : null}
    </div>
  );
}
