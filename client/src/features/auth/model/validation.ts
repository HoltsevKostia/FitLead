import { z } from "zod";

import type { FormFieldErrors, LoginFormValues, RegisterFormValues } from "@/features/auth/model/types";

const roleValues = ["Trainer", "Client"] as const;

export const loginFormSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, "Вкажи електронну пошту.")
    .regex(z.regexes.html5Email, "Вкажи коректну електронну пошту."),
  password: z.string().min(1, "Вкажи пароль."),
});

const roleSchema = z.enum(roleValues, {
  error: "Обери коректну роль.",
});
const requiredRoleSchema = z
  .string()
  .min(1, "Обери роль.")
  .pipe(roleSchema);

export const registerFormSchema = z.object({
  fullName: z
    .string()
    .trim()
    .min(1, "Вкажи повне ім’я.")
    .max(100, "Ім’я занадто довге."),
  email: z
    .string()
    .trim()
    .min(1, "Вкажи електронну пошту.")
    .regex(z.regexes.html5Email, "Вкажи коректну електронну пошту."),
  password: z
    .string()
    .min(6, "Пароль має містити щонайменше 6 символів.")
    .max(128, "Пароль занадто довгий.")
    .regex(/[A-Z]/, "Пароль має містити хоча б одну велику літеру.")
    .regex(/[a-z]/, "Пароль має містити хоча б одну малу літеру.")
    .regex(/\d/, "Пароль має містити хоча б одну цифру.")
    .regex(/[^a-zA-Z0-9]/, "Пароль має містити хоча б один спеціальний символ."),
  role: requiredRoleSchema,
});

export function getLoginFormValues(formData: FormData): LoginFormValues {
  return {
    email: String(formData.get("email") ?? ""),
    password: String(formData.get("password") ?? ""),
  };
}

export function getRegisterFormValues(formData: FormData): RegisterFormValues {
  return {
    fullName: String(formData.get("fullName") ?? ""),
    email: String(formData.get("email") ?? ""),
    password: String(formData.get("password") ?? ""),
    role: String(formData.get("role") ?? "") as RegisterFormValues["role"],
  };
}

export function getFieldErrors<TValues>(error: z.ZodError): FormFieldErrors<TValues> {
  const flattenedErrors = z.flattenError(error)
    .fieldErrors as Record<string, string[] | undefined>;
  const result = {} as FormFieldErrors<TValues>;

  for (const [field, messages] of Object.entries(flattenedErrors)) {
    const firstMessage = messages?.[0];
    if (firstMessage) {
      result[field as keyof TValues] = firstMessage;
    }
  }

  return result;
}
