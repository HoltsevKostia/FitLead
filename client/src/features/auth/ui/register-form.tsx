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
import { FormAlert } from "@/shared/forms/form-alert";
import { PasswordField } from "@/shared/forms/password-field";
import { TextField } from "@/shared/forms/text-field";
import { fieldErrorClassName, fieldLabelClassName } from "@/shared/forms/field-styles";

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
      <TextField
        id="register-full-name"
        name="fullName"
        type="text"
        label="Повне ім’я"
        autoComplete="name"
        required
        maxLength={100}
        value={values.fullName}
        onChange={(event) => updateField("fullName", event.currentTarget.value)}
        disabled={isSubmitting}
        placeholder="Ім’я та прізвище"
        error={fieldErrors.fullName}
      />

      <TextField
        id="register-email"
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
        id="register-password"
        name="password"
        label="Пароль"
        autoComplete="new-password"
        required
        minLength={6}
        maxLength={128}
        value={values.password}
        onChange={(event) => updateField("password", event.currentTarget.value)}
        disabled={isSubmitting}
        placeholder="Створи пароль"
        error={fieldErrors.password}
      />

      <fieldset
        className="space-y-3"
        aria-describedby={fieldErrors.role ? "register-role-error" : undefined}
      >
        <legend className={fieldLabelClassName}>Роль</legend>
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
          <p id="register-role-error" className={fieldErrorClassName}>
            {fieldErrors.role}
          </p>
        ) : null}
      </fieldset>

      <FormAlert message={submitError} />

      <RegisterSubmitButton isSubmitting={isSubmitting} />
    </form>
  );
}
