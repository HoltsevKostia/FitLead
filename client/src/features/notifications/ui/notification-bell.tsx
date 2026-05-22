"use client";

import type { Notification } from "@/entities/notification/model/types";
import {
  getPushNotificationAvailability,
  hasActivePushSubscription,
  subscribeToPushNotifications,
  unsubscribeFromPushNotifications,
  type PushNotificationAvailability,
} from "@/features/notifications/model/push-subscription";
import { notificationsApi } from "@/lib/api/clients/notifications-api";
import { createNotificationConnection } from "@/lib/realtime/notification-connection";
import { resolveSafeNextHref } from "@/shared/utils/resolve-safe-next-href";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";

const UI_TEXT = {
  label: "Сповіщення",
  title: "Сповіщення",
  markAllRead: "Прочитати всі",
  enablePush: "Увімкнути push",
  disablePush: "Вимкнути push",
  pushDenied: "Дозвіл на push заблоковано",
  pushUnsupported: "Push недоступні у цьому браузері",
  pushError: "Не вдалося увімкнути push.",
  pushDisableError: "Не вдалося вимкнути push.",
  empty: "Сповіщень немає",
  error: "Не вдалося завантажити сповіщення.",
} as const;

const NOTIFICATION_LIST_LIMIT = 10;
const DROPDOWN_MAX_WIDTH = 352;
const DROPDOWN_VIEWPORT_GAP = 16;
const DROPDOWN_TOP_OFFSET = 12;

const notificationTimeFormatter = new Intl.DateTimeFormat("uk-UA", {
  day: "2-digit",
  month: "2-digit",
  hour: "2-digit",
  minute: "2-digit",
});

type PushButtonState =
  | PushNotificationAvailability
  | "subscribing"
  | "subscribed"
  | "unsubscribing"
  | "error";

interface DropdownPosition {
  left: number;
  top: number;
  width: number;
  maxHeight: number;
}

function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}

function formatNotificationTime(value: string): string {
  return notificationTimeFormatter.format(new Date(value));
}

function BellIcon() {
  return (
    <svg
      aria-hidden="true"
      className="h-5 w-5"
      fill="none"
      stroke="currentColor"
      strokeLinecap="round"
      strokeLinejoin="round"
      strokeWidth="1.8"
      viewBox="0 0 24 24"
    >
      <path d="M15 17H9" />
      <path d="M18 8A6 6 0 0 0 6 8c0 7-3 7-3 9h18c0-2-3-2-3-9" />
      <path d="M13.73 21a2 2 0 0 1-3.46 0" />
    </svg>
  );
}

function getPushButtonText(state: PushButtonState): string {
  if (state === "subscribing") {
    return "Вмикаємо...";
  }

  if (state === "unsubscribing") {
    return "Вимикаємо...";
  }

  if (state === "subscribed") {
    return UI_TEXT.disablePush;
  }

  if (state === "denied") {
    return UI_TEXT.pushDenied;
  }

  if (state === "unsupported") {
    return UI_TEXT.pushUnsupported;
  }

  return UI_TEXT.enablePush;
}

export function NotificationBell() {
  const router = useRouter();
  const triggerRef = useRef<HTMLButtonElement | null>(null);
  const dropdownRef = useRef<HTMLDivElement | null>(null);
  const seenNotificationIdsRef = useRef<Set<string>>(new Set());
  const [isOpen, setIsOpen] = useState(false);
  const [dropdownPosition, setDropdownPosition] = useState<DropdownPosition>({
    left: DROPDOWN_VIEWPORT_GAP,
    top: DROPDOWN_VIEWPORT_GAP,
    width: DROPDOWN_MAX_WIDTH,
    maxHeight: 320,
  });
  const [isLoading, setIsLoading] = useState(false);
  const [isMarkingAllRead, setIsMarkingAllRead] = useState(false);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const [pushButtonState, setPushButtonState] = useState<PushButtonState>(() =>
    getPushNotificationAvailability(),
  );

  const hasUnreadNotifications = unreadCount > 0;

  const isPushButtonDisabled =
    pushButtonState === "unsupported" ||
    pushButtonState === "denied" ||
    pushButtonState === "subscribing" ||
    pushButtonState === "unsubscribing";

  function updateDropdownPosition() {
    const trigger = triggerRef.current;
    if (!trigger) {
      return;
    }

    const triggerRect = trigger.getBoundingClientRect();
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const availableWidth = Math.max(
      0,
      viewportWidth - DROPDOWN_VIEWPORT_GAP * 2,
    );
    const width = Math.min(DROPDOWN_MAX_WIDTH, availableWidth);
    const maxLeft = viewportWidth - width - DROPDOWN_VIEWPORT_GAP;
    const preferredLeft = triggerRect.right - width;
    const left = clamp(
      preferredLeft,
      DROPDOWN_VIEWPORT_GAP,
      Math.max(maxLeft, DROPDOWN_VIEWPORT_GAP),
    );
    const preferredTop = triggerRect.bottom + DROPDOWN_TOP_OFFSET;
    const top = clamp(
      preferredTop,
      DROPDOWN_VIEWPORT_GAP,
      Math.max(viewportHeight - DROPDOWN_VIEWPORT_GAP, DROPDOWN_VIEWPORT_GAP),
    );
    const maxHeight = Math.max(220, viewportHeight - top - DROPDOWN_VIEWPORT_GAP);

    setDropdownPosition({
      left,
      top,
      width,
      maxHeight,
    });
  }

  useEffect(() => {
    let ignore = false;

    async function syncPushState() {
      const availability = getPushNotificationAvailability();
      if (availability !== "available") {
        setPushButtonState(availability);
        return;
      }

      const hasSubscription = await hasActivePushSubscription();
      if (!ignore) {
        setPushButtonState(hasSubscription ? "subscribed" : "available");
      }
    }

    void syncPushState();

    return () => {
      ignore = true;
    };
  }, []);

  useEffect(() => {
    const connection = createNotificationConnection();
    let isDisposed = false;

    function handleNotificationCreated(notification: Notification) {
      if (isDisposed) {
        return;
      }

      if (seenNotificationIdsRef.current.has(notification.id)) {
        return;
      }

      seenNotificationIdsRef.current.add(notification.id);

      if (!notification.isRead) {
        setUnreadCount((currentCount) => currentCount + 1);
      }

      setNotifications((currentNotifications) => {
        if (currentNotifications.some((current) => current.id === notification.id)) {
          return currentNotifications;
        }

        return [notification, ...currentNotifications].slice(0, NOTIFICATION_LIST_LIMIT);
      });
    }

    async function startConnection() {
      connection.on("NotificationCreated", handleNotificationCreated);

      try {
        await connection.start();
      } catch {
      }
    }

    const startPromise = startConnection();

    return () => {
      isDisposed = true;
      connection.off("NotificationCreated", handleNotificationCreated);
      void startPromise.finally(() => connection.stop());
    };
  }, []);

  useEffect(() => {
    let ignore = false;

    async function loadUnreadCount() {
      try {
        const result = await notificationsApi.getUnreadCount();
        if (!ignore) {
          setUnreadCount(result.count);
        }
      } catch {
        if (!ignore) {
          setUnreadCount(0);
        }
      }
    }

    void loadUnreadCount();

    return () => {
      ignore = true;
    };
  }, []);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    let ignore = false;

    async function loadNotifications() {
      setIsLoading(true);
      setError(null);

      try {
        const result = await notificationsApi.getNotifications(NOTIFICATION_LIST_LIMIT);
        if (!ignore) {
          setNotifications(result);
          seenNotificationIdsRef.current = new Set(
            result.map((notification) => notification.id),
          );
        }
      } catch {
        if (!ignore) {
          setError(UI_TEXT.error);
        }
      } finally {
        if (!ignore) {
          setIsLoading(false);
        }
      }
    }

    void loadNotifications();

    return () => {
      ignore = true;
    };
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    function handlePointerDown(event: PointerEvent) {
      const target = event.target as Node;
      if (
        dropdownRef.current?.contains(target) ||
        triggerRef.current?.contains(target)
      ) {
        return;
      }

      setIsOpen(false);
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setIsOpen(false);
      }
    }

    function handleViewportChange() {
      updateDropdownPosition();
    }

    const animationFrame = window.requestAnimationFrame(handleViewportChange);

    document.addEventListener("pointerdown", handlePointerDown);
    document.addEventListener("keydown", handleKeyDown);
    window.addEventListener("resize", handleViewportChange);
    window.addEventListener("scroll", handleViewportChange, true);

    return () => {
      window.cancelAnimationFrame(animationFrame);
      document.removeEventListener("pointerdown", handlePointerDown);
      document.removeEventListener("keydown", handleKeyDown);
      window.removeEventListener("resize", handleViewportChange);
      window.removeEventListener("scroll", handleViewportChange, true);
    };
  }, [isOpen]);

  function handleOpenChange() {
    setIsOpen((current) => {
      if (!current) {
        updateDropdownPosition();
      }

      return !current;
    });
  }

  async function handleEnablePush() {
    if (pushButtonState === "subscribed") {
      await handleDisablePush();
      return;
    }

    setPushButtonState("subscribing");
    setError(null);

    try {
      await subscribeToPushNotifications();
      setPushButtonState("subscribed");
    } catch {
      const availability = getPushNotificationAvailability();
      setPushButtonState(availability === "available" ? "error" : availability);
      setError(UI_TEXT.pushError);
    }
  }

  async function handleDisablePush() {
    setPushButtonState("unsubscribing");
    setError(null);

    try {
      await unsubscribeFromPushNotifications();
      setPushButtonState("available");
    } catch {
      setPushButtonState("subscribed");
      setError(UI_TEXT.pushDisableError);
    }
  }

  async function handleNotificationClick(notification: Notification) {
    const href = resolveSafeNextHref(notification.linkUrl, "/dashboard");

    if (!notification.isRead) {
      try {
        await notificationsApi.markRead(notification.id);
        setNotifications((currentNotifications) =>
          currentNotifications.map((currentNotification) =>
            currentNotification.id === notification.id
              ? { ...currentNotification, isRead: true, readAtUtc: new Date().toISOString() }
              : currentNotification,
          ),
        );
        setUnreadCount((currentCount) => Math.max(0, currentCount - 1));
      } catch {
        setError(UI_TEXT.error);
        return;
      }
    }

    setIsOpen(false);
    router.push(href);
  }

  async function handleMarkAllRead() {
    setIsMarkingAllRead(true);
    setError(null);

    try {
      await notificationsApi.markAllRead();
      const readAtUtc = new Date().toISOString();
      setNotifications((currentNotifications) =>
        currentNotifications.map((notification) => ({
          ...notification,
          isRead: true,
          readAtUtc: notification.readAtUtc ?? readAtUtc,
        })),
      );
      setUnreadCount(0);
    } catch {
      setError(UI_TEXT.error);
    } finally {
      setIsMarkingAllRead(false);
    }
  }

  return (
    <div className="relative">
      <button
        ref={triggerRef}
        type="button"
        onClick={handleOpenChange}
        className="relative inline-flex h-10 w-10 items-center justify-center rounded-full border border-border bg-surface text-foreground transition hover:bg-surface-strong"
        aria-label={UI_TEXT.label}
        aria-expanded={isOpen}
      >
        <BellIcon />
        {unreadCount > 0 ? (
          <span className="absolute -right-1 -top-1 min-w-5 rounded-full bg-accent px-1.5 py-0.5 text-center text-[11px] font-semibold leading-none text-white">
            {unreadCount > 99 ? "99+" : unreadCount}
          </span>
        ) : null}
      </button>

      {isOpen ? (
        <div
          ref={dropdownRef}
          className="fixed z-20 overflow-hidden rounded-2xl border border-border bg-surface shadow-xl"
          style={{
            left: dropdownPosition.left,
            top: dropdownPosition.top,
            width: dropdownPosition.width,
            maxHeight: dropdownPosition.maxHeight,
          }}
        >
          <div className="flex items-center justify-between gap-3 border-b border-border px-4 py-3">
            <p className="text-sm font-semibold">{UI_TEXT.title}</p>
            <button
              type="button"
              onClick={handleMarkAllRead}
              disabled={!hasUnreadNotifications || isMarkingAllRead}
              className="text-xs font-medium text-accent transition hover:text-accent-strong disabled:cursor-not-allowed disabled:text-muted"
            >
              {UI_TEXT.markAllRead}
            </button>
          </div>

          <div className="border-b border-border px-4 py-3">
            <button
              type="button"
              onClick={() => void handleEnablePush()}
              disabled={isPushButtonDisabled}
              className="w-full rounded-xl border border-border px-3 py-2 text-sm font-medium transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-60"
            >
              {getPushButtonText(pushButtonState)}
            </button>
          </div>

          <div
            className="overflow-y-auto"
            style={{ maxHeight: Math.max(160, dropdownPosition.maxHeight - 116) }}
          >
            {isLoading ? (
              <div className="space-y-3 p-4">
                <div className="h-16 animate-pulse rounded-xl bg-surface-strong" />
                <div className="h-16 animate-pulse rounded-xl bg-surface-strong" />
              </div>
            ) : error ? (
              <p className="px-4 py-6 text-sm text-muted">{error}</p>
            ) : notifications.length === 0 ? (
              <p className="px-4 py-6 text-sm text-muted">{UI_TEXT.empty}</p>
            ) : (
              <ul className="divide-y divide-border">
                {notifications.map((notification) => (
                  <li key={notification.id}>
                    <button
                      type="button"
                      onClick={() => void handleNotificationClick(notification)}
                      className="flex w-full gap-3 px-4 py-3 text-left transition hover:bg-surface-strong"
                    >
                      <span
                        className={`mt-1 h-2.5 w-2.5 shrink-0 rounded-full ${
                          notification.isRead ? "bg-border" : "bg-accent"
                        }`}
                      />
                      <span className="min-w-0 flex-1">
                        <span className="block truncate text-sm font-medium">
                          {notification.title}
                        </span>
                        {notification.body ? (
                          <span className="mt-1 line-clamp-2 block text-sm text-muted">
                            {notification.body}
                          </span>
                        ) : null}
                        <span className="mt-2 block text-xs text-muted">
                          {formatNotificationTime(notification.createdAtUtc)}
                        </span>
                      </span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      ) : null}
    </div>
  );
}
