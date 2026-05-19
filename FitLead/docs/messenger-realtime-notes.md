# Messenger Real-Time Notes

- Database remains the source of truth. SignalR only delivers best-effort live notifications after committed writes.
- Browser MVP reuses existing auth cookies that contain the JWT. Do not add a separate browser `accessTokenFactory` unless the auth model changes.
- Hub access checks must reuse shared chat-access rules, but hub code should not depend on an HTTP-only current-user flow. Prefer an access API that accepts an explicit user id from hub claims.
- `MessageCreated` should carry the UI-ready message DTO, including sender name.
- Send `MessageCreated` to the whole chat group, including the sender; frontend deduplicates by message id.
- Rejoin chat groups after reconnect.
- Group-per-chat covers live delivery for an open chat. User-level delivery for chat-list updates and unread counters is a future extension.
- Redis backplane is future scale-out infrastructure, not durable delivery.
