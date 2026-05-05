export type UserRole = "Trainer" | "Client";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  role: UserRole;
}

export interface AuthSession {
  expiresIn: number;
}

export interface CurrentUser {
  id: string;
  email: string;
  role: UserRole;
}

export interface LoginFormValues {
  email: string;
  password: string;
}

export interface RegisterFormValues {
  fullName: string;
  email: string;
  password: string;
  role: UserRole | "";
}

export type FormFieldErrors<TValues> = Partial<Record<keyof TValues, string>>;

export interface AuthFormState<TValues> {
  fieldErrors: FormFieldErrors<TValues>;
  submitError: string | null;
}
