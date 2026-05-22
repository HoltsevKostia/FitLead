import type {
  PushSubscriptionRegistration,
  RegisterPushSubscriptionRequest,
  RevokePushSubscriptionRequest,
  VapidPublicKey,
} from "@/entities/notification/model/push-types";
import { apiRequest } from "@/lib/api/http-client";

export const pushApi = {
  getVapidPublicKey(): Promise<VapidPublicKey> {
    return apiRequest<VapidPublicKey>("/api/push/vapid-public-key");
  },

  registerSubscription(
    request: RegisterPushSubscriptionRequest,
  ): Promise<PushSubscriptionRegistration> {
    return apiRequest<PushSubscriptionRegistration>("/api/push/subscriptions", {
      method: "POST",
      body: request,
    });
  },

  revokeCurrentSubscription(request: RevokePushSubscriptionRequest): Promise<void> {
    return apiRequest<void>("/api/push/subscriptions/current/revoke", {
      method: "POST",
      body: request,
    });
  },
};
