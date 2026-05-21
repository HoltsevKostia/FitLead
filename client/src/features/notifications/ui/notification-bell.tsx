"use client";

import type { Notification } from "@/entities/notification/model/types";
import { notificationsApi } from "@/lib/api/clients/notifications-api";
import { resolveSafeNextHref } from "@/shared/utils/resolve-safe-next-href";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useRef, useState } from "react";

const UI_TEXT = {
  label: "Сповіщення",
  title: "Сповіщення",
  markAllRead: "Прочитати всі",
  empty: "Нових сповіщень немає",
  error: "Не вдалося завантажити сповіщення.",
} as const;

const notificationTimeFormatter = new Intl.DateTimeFormat("uk-UA", {
  day: "2-digit",
  month: "2-digit",
  hour: "2-digit",
  minute: "2-digit",
});

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

export function NotificationBell() {
  const router = useRouter();
  const dropdownRef = useRef<HTMLDivElement | null>(null);
  const [isOpen, setIsOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isMarkingAllRead, setIsMarkingAllRead] = useState(false);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [error, setError] = useState<string | null>(null);

  const hasUnreadNotifications = useMemo(
    () => notifications.some((notification) => !notification.isRead),
    [notifications],
  );

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
        const result = await notificationsApi.getNotifications(10);
        if (!ignore) {
          setNotifications(result);
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
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setIsOpen(false);
      }
    }

    document.addEventListener("pointerdown", handlePointerDown);
    document.addEventListener("keydown", handleKeyDown);

    return () => {
      document.removeEventListener("pointerdown", handlePointerDown);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [isOpen]);

  async function handleOpenChange() {
    setIsOpen((current) => !current);
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
    <div ref={dropdownRef} className="relative">
      <button
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
        <div className="absolute right-0 z-20 mt-3 w-[min(22rem,calc(100vw-2rem))] overflow-hidden rounded-2xl border border-border bg-surface shadow-xl">
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

          <div className="max-h-96 overflow-y-auto">
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
