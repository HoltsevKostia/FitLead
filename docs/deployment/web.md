# Web Deployment Notes

## BFF Routing

The public browser origin is `https://fitlead.space`.

Browser business API requests go through the Next BFF:

- `/api/backend/*`

Browser auth requests go through the Next BFF:

- `/api/auth/*`

Next server-side route handlers and Server Components call ASP.NET internally
through `API_BASE_URL`, for example:

- `http://fitlead-api:8080`

Do not expose this internal URL through `NEXT_PUBLIC_*` variables.

## SignalR Routing

Browser SignalR clients use same-origin hub paths in production:

- `/hubs/chat`
- `/hubs/notifications`

Dokploy/Traefik must route `/hubs/*` to the ASP.NET API service with WebSocket
upgrade support. Do not proxy SignalR hubs through Next route handlers.

For local development, `NEXT_PUBLIC_SIGNALR_BASE_URL` can point to the backend
development server, for example `http://localhost:5178`.
