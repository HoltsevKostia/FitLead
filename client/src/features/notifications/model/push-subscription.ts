import { pushApi } from "@/lib/api/clients/push-api";

export type PushNotificationAvailability =
  | "available"
  | "denied"
  | "unsupported";

export function getPushNotificationAvailability(): PushNotificationAvailability {
  if (
    typeof window === "undefined" ||
    !("serviceWorker" in navigator) ||
    !("PushManager" in window) ||
    !("Notification" in window)
  ) {
    return "unsupported";
  }

  if (globalThis.Notification.permission === "denied") {
    return "denied";
  }

  return "available";
}

export async function hasActivePushSubscription(): Promise<boolean> {
  if (getPushNotificationAvailability() !== "available") {
    return false;
  }

  const registration = await navigator.serviceWorker.getRegistration("/sw.js");
  if (!registration) {
    return false;
  }

  const subscription = await registration.pushManager.getSubscription();
  return subscription !== null;
}

function urlBase64ToArrayBuffer(value: string): ArrayBuffer {
  const padding = "=".repeat((4 - (value.length % 4)) % 4);
  const base64 = `${value}${padding}`.replace(/-/g, "+").replace(/_/g, "/");
  const rawData = window.atob(base64);
  const buffer = new ArrayBuffer(rawData.length);
  const output = new Uint8Array(buffer);

  for (let index = 0; index < rawData.length; index += 1) {
    output[index] = rawData.charCodeAt(index);
  }

  return buffer;
}

export async function subscribeToPushNotifications(): Promise<void> {
  const availability = getPushNotificationAvailability();
  if (availability === "unsupported") {
    throw new Error("Push notifications are not supported.");
  }

  if (availability === "denied") {
    throw new Error("Push notification permission is blocked.");
  }

  const permission = await globalThis.Notification.requestPermission();
  if (permission !== "granted") {
    throw new Error("Push notification permission was not granted.");
  }

  const registration = await navigator.serviceWorker.register("/sw.js");
  const existingSubscription = await registration.pushManager.getSubscription();

  const subscription =
    existingSubscription ??
    (await (async () => {
      const { publicKey } = await pushApi.getVapidPublicKey();

      return registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToArrayBuffer(publicKey),
      });
    })());

  const serializedSubscription = subscription.toJSON();
  const p256dh = serializedSubscription.keys?.p256dh;
  const auth = serializedSubscription.keys?.auth;

  if (!p256dh || !auth) {
    throw new Error("Push subscription keys are missing.");
  }

  await pushApi.registerSubscription({
    endpoint: subscription.endpoint,
    keys: {
      p256dh,
      auth,
    },
    userAgent: navigator.userAgent || null,
  });
}

export async function unsubscribeFromPushNotifications(): Promise<void> {
  if (getPushNotificationAvailability() !== "available") {
    return;
  }

  const registration = await navigator.serviceWorker.getRegistration("/sw.js");
  if (!registration) {
    return;
  }

  const subscription = await registration.pushManager.getSubscription();
  if (!subscription) {
    return;
  }

  await pushApi.revokeCurrentSubscription({
    endpoint: subscription.endpoint,
  });

  await subscription.unsubscribe();
}
