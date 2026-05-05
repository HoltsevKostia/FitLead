"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";

import type { FormFieldErrors, LoginFormValues } from "@/features/auth/model/types";
import { mapLoginError } from "@/features/auth/model/error-mapping";
import {
  getFieldErrors,
  getLoginFormValues,
  loginFormSchema,
} from "@/features/auth/model/validation";
import { authApi } from "@/lib/api/clients/auth-api";

const inputClassName =
  "w-full rounded-2xl border border-border bg-white px-4 py-3 outline-none transition focus:border-accent disabled:cursor-not-allowed disabled:opacity-70";

const initialValues: LoginFormValues = {
  email: "",
  password: "",
};

function LoginSubmitButton({ isSubmitting }: { isSubmitting: boolean }) {
  return (
    <button
      type="submit"
      disabled={isSubmitting}
      className="w-full rounded-2xl bg-accent px-4 py-3 font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
    >
      {isSubmitting ? "Виконуємо вхід..." : "Увійти"}
    </button>
  );
}

export function LoginForm() {
  const router = useRouter();
  const [values, setValues] = useState<LoginFormValues>(initialValues);
  const [fieldErrors, setFieldErrors] = useState<FormFieldErrors<LoginFormValues>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function updateField<K extends keyof LoginFormValues>(field: K, value: LoginFormValues[K]) {
    setValues((current) => ({
      ...current,
      [field]: value,
    }));

    setFieldErrors((current) => {
      if (!current[field]) {
        return current;
      }

      const next = { ...current };
      delete next[field];
      return next;
    });

    if (submitError) {
      setSubmitError(null);
    }
  }

  async function handleSubmit(formData: FormData) {
    setIsSubmitting(true);

    const parsedValues = getLoginFormValues(formData);
    const validation = loginFormSchema.safeParse(parsedValues);

    if (!validation.success) {
      setFieldErrors(getFieldErrors<LoginFormValues>(validation.error));
      setSubmitError(null);
      setIsSubmitting(false);
      return;
    }

    setFieldErrors({});
    setSubmitError(null);

    try {
      await authApi.login(validation.data);
      router.replace("/dashboard");
      router.refresh();
    } catch (error) {
      const mappedError = mapLoginError(error);
      setFieldErrors(mappedError.fieldErrors);
      setSubmitError(mappedError.submitError);
      setIsSubmitting(false);
    }
  }

  return (
    <form className="space-y-4" noValidate action={handleSubmit}>
      <div className="space-y-2">
        <label className="block text-sm font-medium text-foreground" htmlFor="login-email">
          Електронна пошта
        </label>
        <input
          id="login-email"
          name="email"
          type="email"
          autoComplete="email"
          required
          maxLength={254}
          value={values.email}
          onChange={(event) => updateField("email", event.currentTarget.value)}
          aria-invalid={fieldErrors.email ? "true" : "false"}
          aria-describedby={fieldErrors.email ? "login-email-error" : undefined}
          disabled={isSubmitting}
          className={inputClassName}
          placeholder="name@example.com"
        />
        {fieldErrors.email ? (
          <p id="login-email-error" className="text-sm text-red-700">
            {fieldErrors.email}
          </p>
        ) : null}
      </div>

      <div className="space-y-2">
        <label className="block text-sm font-medium text-foreground" htmlFor="login-password">
          Пароль
        </label>
        <div className="relative">
          <input
            id="login-password"
            name="password"
            type={showPassword ? "text" : "password"}
            autoComplete="current-password"
            required
            minLength={6}
            maxLength={128}
            value={values.password}
            onChange={(event) => updateField("password", event.currentTarget.value)}
            aria-invalid={fieldErrors.password ? "true" : "false"}
            aria-describedby={fieldErrors.password ? "login-password-error" : undefined}
            disabled={isSubmitting}
            className={`${inputClassName} pr-28`}
            placeholder="Введи пароль"
          />
          <button
            type="button"
            onClick={() => setShowPassword((current) => !current)}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-sm font-medium text-muted transition hover:text-foreground focus:outline-none"
            aria-pressed={showPassword}
            aria-label={showPassword ? "Приховати пароль" : "Показати пароль"}
          >
            {showPassword ? "Сховати" : "Показати"}
          </button>
        </div>
        {fieldErrors.password ? (
          <p id="login-password-error" className="text-sm text-red-700">
            {fieldErrors.password}
          </p>
        ) : null}
      </div>

      {submitError ? (
        <p
          role="alert"
          aria-live="polite"
          className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800"
        >
          {submitError}
        </p>
      ) : null}

      <LoginSubmitButton isSubmitting={isSubmitting} />
    </form>
  );
}
