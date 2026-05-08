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
    const session = await apiRequest<AuthSession>("/auth/register", {
      method: "POST",
      body: payload,
    });
    await refreshCsrfToken();
    return session;
  },

  async login(payload: LoginRequest): Promise<AuthSession> {
    const session = await apiRequest<AuthSession>("/auth/login", {
      method: "POST",
      body: payload,
    });
    await refreshCsrfToken();
    return session;
  },

  refresh(): Promise<AuthSession> {
    return apiRequest<AuthSession>("/auth/refresh", {
      method: "POST",
    });
  },

  async logout(): Promise<void> {
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
