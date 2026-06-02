# Deployment Notes

## SignalR routing

Browser SignalR clients use same-origin hub paths in production:

- `/hubs/chat`
- `/hubs/notifications`

Dokploy/Traefik must route `/hubs/*` to the ASP.NET API service with WebSocket
upgrade support. Do not proxy SignalR hubs through Next route handlers.

For local development, `NEXT_PUBLIC_SIGNALR_BASE_URL` can point to the backend
development server, for example `http://localhost:5178`.
