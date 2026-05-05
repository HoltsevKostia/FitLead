import type {
  AuthFormState,
  FormFieldErrors,
  LoginFormValues,
  RegisterFormValues,
} from "@/features/auth/model/types";
import { isApiError } from "@/lib/api/api-error";

const loginFieldNameMap = {
  email: "email",
  password: "password",
} as const satisfies Partial<Record<string, keyof LoginFormValues>>;

const registerFieldNameMap = {
  email: "email",
  password: "password",
  fullname: "fullName",
  role: "role",
} as const satisfies Partial<Record<string, keyof RegisterFormValues>>;

function normalizeFieldKey(field: string): string {
  const lastSegment = field.split(".").at(-1) ?? field;

  return lastSegment.replace(/^\$\./, "").toLowerCase();
}

function hasFieldErrors<TValues>(
  fieldErrors: FormFieldErrors<TValues>,
): boolean {
  return Object.keys(fieldErrors).length > 0;
}

function mapFieldErrors<TValues>(
  errors: Record<string, string[]> | null,
  fieldMap: Partial<Record<string, keyof TValues>>,
): FormFieldErrors<TValues> {
  const result = {} as FormFieldErrors<TValues>;

  if (!errors) {
    return result;
  }

  for (const [field, messages] of Object.entries(errors)) {
    const mappedField = fieldMap[normalizeFieldKey(field)];
    const firstMessage = messages[0];

    if (!mappedField || !firstMessage) {
      continue;
    }

    result[mappedField] = firstMessage;
  }

  return result;
}

function submitFailure<TValues>(message: string): AuthFormState<TValues> {
  return {
    fieldErrors: {},
    submitError: message,
  };
}

function fieldFailure<TValues>(
  fieldErrors: FormFieldErrors<TValues>,
): AuthFormState<TValues> {
  return {
    fieldErrors,
    submitError: null,
  };
}

function defaultSubmitError<TValues>(
  status: number,
  detail: string | null,
  fallbackMessage: string,
): AuthFormState<TValues> {
  if (status >= 500) {
    return submitFailure("На сервері сталася помилка. Спробуй пізніше.");
  }

  if (status === 429) {
    return submitFailure("Забагато спроб. Спробуй пізніше.");
  }

  return submitFailure(detail ?? fallbackMessage);
}

export function mapLoginError(error: unknown): AuthFormState<LoginFormValues> {
  const fallbackMessage = "Не вдалося виконати вхід. Спробуй ще раз.";

  if (!isApiError(error)) {
    return submitFailure(fallbackMessage);
  }

  if (error.status === 401) {
    return submitFailure("Неправильна електронна пошта або пароль.");
  }

  const fieldErrors = mapFieldErrors<LoginFormValues>(
    error.errors,
    loginFieldNameMap,
  );

  if (hasFieldErrors(fieldErrors)) {
    return fieldFailure(fieldErrors);
  }

  return defaultSubmitError(error.status, error.detail, fallbackMessage);
}

export function mapRegisterError(error: unknown): AuthFormState<RegisterFormValues> {
  const fallbackMessage = "Не вдалося створити акаунт. Спробуй ще раз.";

  if (!isApiError(error)) {
    return submitFailure(fallbackMessage);
  }

  const fieldErrors = mapFieldErrors<RegisterFormValues>(
    error.errors,
    registerFieldNameMap,
  );

  switch (error.errorCode) {
    case "auth.email_exists":
      return fieldFailure({
        ...fieldErrors,
        email: "Користувач із такою електронною поштою вже існує.",
      });
    case "auth.email_required":
      return fieldFailure({
        ...fieldErrors,
        email: "Вкажи електронну пошту.",
      });
    case "auth.email_invalid":
      return fieldFailure({
        ...fieldErrors,
        email: "Вкажи коректну електронну пошту.",
      });
    case "auth.password_required":
      return fieldFailure({
        ...fieldErrors,
        password: "Вкажи пароль.",
      });
    case "auth.full_name_required":
      return fieldFailure({
        ...fieldErrors,
        fullName: "Вкажи повне ім’я.",
      });
    case "auth.role_required":
      return fieldFailure({
        ...fieldErrors,
        role: "Обери роль.",
      });
    case "auth.role_invalid":
      return fieldFailure({
        ...fieldErrors,
        role: "Обери коректну роль.",
      });
  }

  if (hasFieldErrors(fieldErrors)) {
    return fieldFailure(fieldErrors);
  }

  return defaultSubmitError(error.status, error.detail, fallbackMessage);
}