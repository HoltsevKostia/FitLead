"use client";

import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";

import { buildHubUrl } from "@/lib/realtime/hub-url";

function getNotificationHubUrl(): string {
  return buildHubUrl("/hubs/notifications");
}

export function createNotificationConnection(): HubConnection {
  const builder = new HubConnectionBuilder()
    .withUrl(getNotificationHubUrl())
    .withAutomaticReconnect();

  if (process.env.NODE_ENV === "development") {
    builder.configureLogging(LogLevel.Information);
  }

  return builder.build();
}
