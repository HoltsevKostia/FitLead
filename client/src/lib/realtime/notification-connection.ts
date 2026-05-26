"use client";

import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";

import { apiEnv } from "@/lib/api/env";

function getNotificationHubUrl(): string {
  return new URL("/hubs/notifications", apiEnv.baseUrl).toString();
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
