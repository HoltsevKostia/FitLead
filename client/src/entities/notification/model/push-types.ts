export interface VapidPublicKey {
  publicKey: string;
}

export interface RegisterPushSubscriptionRequest {
  endpoint: string;
  keys: {
    p256dh: string;
    auth: string;
  };
  userAgent: string | null;
}

export interface RevokePushSubscriptionRequest {
  endpoint: string;
}

export interface PushSubscriptionRegistration {
  id: string;
  endpoint: string;
  createdAtUtc: string;
}
