"use client";

import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";

import { buildHubUrl } from "@/lib/realtime/hub-url";

function getChatHubUrl(): string {
  return buildHubUrl("/hubs/chat");
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
