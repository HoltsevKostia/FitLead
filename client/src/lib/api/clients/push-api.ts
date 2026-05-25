import type {
  PushSubscriptionRegistration,
  RegisterPushSubscriptionRequest,
  RevokePushSubscriptionRequest,
  VapidPublicKey,
} from "@/entities/notification/model/push-types";
import { apiRequest } from "@/lib/api/http-client";

export const pushApi = {
  getVapidPublicKey(): Promise<VapidPublicKey> {
    return apiRequest<VapidPublicKey>("/push/vapid-public-key");
  },

  registerSubscription(
    request: RegisterPushSubscriptionRequest,
  ): Promise<PushSubscriptionRegistration> {
    return apiRequest<PushSubscriptionRegistration>("/push/subscriptions", {
      method: "POST",
      body: request,
    });
  },

  revokeCurrentSubscription(request: RevokePushSubscriptionRequest): Promise<void> {
    return apiRequest<void>("/push/subscriptions/current/revoke", {
      method: "POST",
      body: request,
    });
  },
};
