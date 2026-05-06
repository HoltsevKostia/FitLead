import type {
  AuthSession,
  CurrentUser,
  LoginRequest,
  RegisterRequest,
} from "@/features/auth/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const authApi = {
  register(payload: RegisterRequest): Promise<AuthSession> {
    return apiRequest<AuthSession>("/auth/register", {
      method: "POST",
      body: payload,
    });
  },

  login(payload: LoginRequest): Promise<AuthSession> {
    return apiRequest<AuthSession>("/auth/login", {
      method: "POST",
      body: payload,
    });
  },

  refresh(): Promise<AuthSession> {
    return apiRequest<AuthSession>("/auth/refresh", {
      method: "POST",
    });
  },

  logout(): Promise<void> {
    return apiRequest<void>("/auth/logout", {
      method: "POST",
      responseType: "void",
    });
  },

  getCurrentUser(): Promise<CurrentUser> {
    return apiRequest<CurrentUser>("/auth/current-user");
  },
};
