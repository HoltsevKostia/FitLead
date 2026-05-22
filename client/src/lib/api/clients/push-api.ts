import type {
  PushSubscriptionRegistration,
  RegisterPushSubscriptionRequest,
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
};
