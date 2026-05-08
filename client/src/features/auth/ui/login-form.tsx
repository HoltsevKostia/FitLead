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
import { FormAlert } from "@/shared/forms/form-alert";
import { PasswordField } from "@/shared/forms/password-field";
import { TextField } from "@/shared/forms/text-field";

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

interface LoginFormProps {
  nextHref?: string;
}

export function LoginForm({ nextHref = "/dashboard" }: LoginFormProps) {
  const router = useRouter();
  const [values, setValues] = useState<LoginFormValues>(initialValues);
  const [fieldErrors, setFieldErrors] = useState<FormFieldErrors<LoginFormValues>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
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
      router.replace(nextHref);
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
      <TextField
        id="login-email"
        name="email"
        type="email"
        label="Електронна пошта"
        autoComplete="email"
        required
        maxLength={254}
        value={values.email}
        onChange={(event) => updateField("email", event.currentTarget.value)}
        disabled={isSubmitting}
        placeholder="name@example.com"
        error={fieldErrors.email}
      />

      <PasswordField
        id="login-password"
        name="password"
        label="Пароль"
        autoComplete="current-password"
        required
        minLength={6}
        maxLength={128}
        value={values.password}
        onChange={(event) => updateField("password", event.currentTarget.value)}
        disabled={isSubmitting}
        placeholder="Введи пароль"
        error={fieldErrors.password}
      />

      <FormAlert message={submitError} />

      <LoginSubmitButton isSubmitting={isSubmitting} />
    </form>
  );
}
