import type {
  AuthSession,
  CurrentUser,
  LoginRequest,
  RegisterRequest,
} from "@/features/auth/model/types";
import { refreshCsrfToken } from "@/lib/api/csrf";
import { apiRequest } from "@/lib/api/http-client";

export const authApi = {
  async register(payload: RegisterRequest): Promise<AuthSession> {
    await refreshCsrfToken();
    const session = await apiRequest<AuthSession>("/auth/register", {
      method: "POST",
      body: payload,
    });
    await refreshCsrfToken();
    return session;
  },

  async login(payload: LoginRequest): Promise<AuthSession> {
    await refreshCsrfToken();
    const session = await apiRequest<AuthSession>("/auth/login", {
      method: "POST",
      body: payload,
    });
    await refreshCsrfToken();
    return session;
  },

  async refresh(): Promise<AuthSession> {
    await refreshCsrfToken();
    const session = await apiRequest<AuthSession>("/auth/refresh", {
      method: "POST",
    });
    await refreshCsrfToken();
    return session;
  },

  async logout(): Promise<void> {
    await refreshCsrfToken();
    await apiRequest<void>("/auth/logout", {
      method: "POST",
      responseType: "void",
    });
    await refreshCsrfToken();
  },

  getCurrentUser(): Promise<CurrentUser> {
    return apiRequest<CurrentUser>("/auth/current-user");
  },
};
