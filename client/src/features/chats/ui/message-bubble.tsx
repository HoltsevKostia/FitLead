import type { ChatMessage } from "@/entities/chat/model/types";
import type { CurrentUser } from "@/features/auth/model/types";
import { formatMessageTime } from "@/features/chats/ui/message-time";

interface MessageBubbleProps {
  currentUser: CurrentUser;
  message: ChatMessage;
}

export function MessageBubble({ currentUser, message }: MessageBubbleProps) {
  const isOwn = message.senderId === currentUser.id;

  return (
    <div className={`flex min-w-0 ${isOwn ? "justify-end" : "justify-start"}`}>
      <article
        className={`min-w-0 max-w-[min(88%,42rem)] rounded-2xl px-4 py-3 sm:max-w-[min(78%,42rem)] ${
          isOwn
            ? "rounded-br-md bg-accent text-white"
            : "rounded-bl-md border border-border bg-surface text-foreground"
        }`}
      >
        {!isOwn ? (
          <p className="mb-1 text-xs font-medium text-muted">{message.senderName}</p>
        ) : null}
        <p className="whitespace-pre-wrap break-words text-sm leading-6">
          {message.text ?? ""}
        </p>
        <p
          className={`mt-2 text-right text-xs ${
            isOwn ? "text-white/75" : "text-muted"
          }`}
        >
          {formatMessageTime(message.createdAtUtc)}
        </p>
      </article>
    </div>
  );
}
