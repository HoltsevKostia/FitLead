# CSRF Protection

## Backend contract
- `GET /auth/csrf-token` issues `XSRF-TOKEN`.
- Unsafe cookie-auth endpoints require antiforgery validation.
- `GET`/read-only endpoints do not require CSRF.

## Frontend contract
- `apiRequest` automatically adds `X-CSRF-TOKEN` for `POST`/`PUT`/`PATCH`/`DELETE`.
- `credentials: "include"` must remain enabled.
- raw `fetch` must not be used for unsafe protected API calls unless CSRF header is added manually.

## Token lifecycle
- Token is bootstrapped through `/auth/csrf-token`.
- Token is refreshed after `login`/`register`/`logout`.
- `XSRF-TOKEN` is readable by JS by design.
- Auth cookies remain `HttpOnly`.

## Adding new endpoints
Checklist:
- Does endpoint change state?
- Does it use cookie auth?
- If yes, add antiforgery validation.
- Add/update integration tests.
- Verify frontend calls go through `apiRequest`.
