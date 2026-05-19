"use client";

import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";

import { apiEnv } from "@/lib/api/env";

function getChatHubUrl(): string {
  return new URL("/hubs/chat", apiEnv.baseUrl).toString();
}

export function createChatConnection(): HubConnection {
  const builder = new HubConnectionBuilder()
    .withUrl(getChatHubUrl())
    .withAutomaticReconnect();

  if (process.env.NODE_ENV === "development") {
    builder.configureLogging(LogLevel.Information);
  }

  return builder.build();
}
