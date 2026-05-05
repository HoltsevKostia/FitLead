"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";

import type {
  FormFieldErrors,
  RegisterFormValues,
  UserRole,
} from "@/features/auth/model/types";
import { mapRegisterError } from "@/features/auth/model/error-mapping";
import {
  getFieldErrors,
  getRegisterFormValues,
  registerFormSchema,
} from "@/features/auth/model/validation";
import { authApi } from "@/lib/api/clients/auth-api";

const inputClassName =
  "w-full rounded-2xl border border-border bg-white px-4 py-3 outline-none transition focus:border-accent disabled:cursor-not-allowed disabled:opacity-70";

const initialValues: RegisterFormValues = {
  fullName: "",
  email: "",
  password: "",
  role: "",
};

const roleOptions: Array<{ value: UserRole; label: string; description: string }> = [
  {
    value: "Trainer",
    label: "Тренер",
    description: "Створює програми, тренування та керує клієнтами.",
  },
  {
    value: "Client",
    label: "Клієнт",
    description: "Отримує програми тренувань і взаємодіє з тренером.",
  },
];

function RegisterSubmitButton({ isSubmitting }: { isSubmitting: boolean }) {
  return (
    <button
      type="submit"
      disabled={isSubmitting}
      className="w-full rounded-2xl bg-accent px-4 py-3 font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
    >
      {isSubmitting ? "Створюємо акаунт..." : "Створити акаунт"}
    </button>
  );
}

export function RegisterForm() {
  const router = useRouter();
  const [values, setValues] = useState<RegisterFormValues>(initialValues);
  const [fieldErrors, setFieldErrors] = useState<FormFieldErrors<RegisterFormValues>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function updateField<K extends keyof RegisterFormValues>(
    field: K,
    value: RegisterFormValues[K],
  ) {
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

    const parsedValues = getRegisterFormValues(formData);
    const validation = registerFormSchema.safeParse(parsedValues);

    if (!validation.success) {
      setFieldErrors(getFieldErrors<RegisterFormValues>(validation.error));
      setSubmitError(null);
      setIsSubmitting(false);
      return;
    }

    setFieldErrors({});
    setSubmitError(null);

    try {
      await authApi.register(validation.data);
      router.replace("/dashboard");
      router.refresh();
    } catch (error) {
      const mappedError = mapRegisterError(error);
      setFieldErrors(mappedError.fieldErrors);
      setSubmitError(mappedError.submitError);
      setIsSubmitting(false);
    }
  }

  return (
    <form className="space-y-4" noValidate action={handleSubmit}>
      <div className="space-y-2">
        <label className="block text-sm font-medium text-foreground" htmlFor="register-full-name">
          Повне ім’я
        </label>
        <input
          id="register-full-name"
          name="fullName"
          type="text"
          autoComplete="name"
          required
          maxLength={100}
          value={values.fullName}
          onChange={(event) => updateField("fullName", event.currentTarget.value)}
          aria-invalid={fieldErrors.fullName ? "true" : "false"}
          aria-describedby={fieldErrors.fullName ? "register-full-name-error" : undefined}
          disabled={isSubmitting}
          className={inputClassName}
          placeholder="Ім’я та прізвище"
        />
        {fieldErrors.fullName ? (
          <p id="register-full-name-error" className="text-sm text-red-700">
            {fieldErrors.fullName}
          </p>
        ) : null}
      </div>

      <div className="space-y-2">
        <label className="block text-sm font-medium text-foreground" htmlFor="register-email">
          Електронна пошта
        </label>
        <input
          id="register-email"
          name="email"
          type="email"
          autoComplete="email"
          required
          maxLength={254}
          value={values.email}
          onChange={(event) => updateField("email", event.currentTarget.value)}
          aria-invalid={fieldErrors.email ? "true" : "false"}
          aria-describedby={fieldErrors.email ? "register-email-error" : undefined}
          disabled={isSubmitting}
          className={inputClassName}
          placeholder="name@example.com"
        />
        {fieldErrors.email ? (
          <p id="register-email-error" className="text-sm text-red-700">
            {fieldErrors.email}
          </p>
        ) : null}
      </div>

      <div className="space-y-2">
        <label className="block text-sm font-medium text-foreground" htmlFor="register-password">
          Пароль
        </label>
        <div className="relative">
          <input
            id="register-password"
            name="password"
            type={showPassword ? "text" : "password"}
            autoComplete="new-password"
            required
            minLength={6}
            maxLength={128}
            value={values.password}
            onChange={(event) => updateField("password", event.currentTarget.value)}
            aria-invalid={fieldErrors.password ? "true" : "false"}
            aria-describedby={fieldErrors.password ? "register-password-error" : undefined}
            disabled={isSubmitting}
            className={`${inputClassName} pr-28`}
            placeholder="Створи пароль"
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
          <p id="register-password-error" className="text-sm text-red-700">
            {fieldErrors.password}
          </p>
        ) : null}
      </div>

      <fieldset
        className="space-y-3"
        aria-describedby={fieldErrors.role ? "register-role-error" : undefined}
      >
        <legend className="text-sm font-medium text-foreground">Роль</legend>
        <div className="grid gap-3">
          {roleOptions.map((option) => {
            const isSelected = values.role === option.value;

            return (
              <label
                key={option.value}
                className={`cursor-pointer rounded-2xl border px-4 py-3 transition focus-within:ring-2 focus-within:ring-accent focus-within:ring-offset-2 ${
                  isSelected
                    ? "border-accent bg-[#eef8f3]"
                    : "border-border bg-white hover:border-accent/60"
                }`}
              >
                <input
                  type="radio"
                  name="role"
                  value={option.value}
                  checked={isSelected}
                  onChange={() => updateField("role", option.value)}
                  disabled={isSubmitting}
                  className="sr-only"
                />
                <span className="block font-medium text-foreground">{option.label}</span>
                <span className="mt-1 block text-sm text-muted">{option.description}</span>
              </label>
            );
          })}
        </div>
        {fieldErrors.role ? (
          <p id="register-role-error" className="text-sm text-red-700">
            {fieldErrors.role}
          </p>
        ) : null}
      </fieldset>

      {submitError ? (
        <p
          role="alert"
          aria-live="polite"
          className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800"
        >
          {submitError}
        </p>
      ) : null}

      <RegisterSubmitButton isSubmitting={isSubmitting} />
    </form>
  );
}
