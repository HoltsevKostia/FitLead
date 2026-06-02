# API Deployment Notes

## Reverse Proxy Headers

FitLead API runs behind Dokploy/Traefik in production. The API enables forwarded
headers for:

- `X-Forwarded-For`
- `X-Forwarded-Proto`
- `X-Forwarded-Host`

`UseForwardedHeaders()` is registered early in the ASP.NET pipeline so
`Request.Scheme` is corrected before HTTPS redirection, auth, CORS, endpoints,
and cookie issuance.

In production the app defaults to trusting the immediate reverse proxy
(`ForwardLimit = 1`) and clears `KnownNetworks`/`KnownProxies`, because Traefik
container IPs are not stable in typical Dokploy Docker networks. This is a
deployment tradeoff. If the proxy IP/network is stable, configure
`ForwardedHeaders:TrustUnknownProxies=false` and set explicit known
proxies/networks in code/config later.

`ForwardedHeaders:AllowedHosts` is restricted to:

- `fitlead.space`
- `www.fitlead.space`

## Cookies

Auth cookies remain host-only. Do not set `Domain=.fitlead.space` by default.
The public browser origin is `https://fitlead.space`, and auth/business HTTP
traffic goes through the Next BFF same-origin routes.

The ASP.NET auth cookies are `HttpOnly`, `SameSite=Lax`, and become `Secure`
when `Request.IsHttps` is true. Behind Traefik this depends on
`X-Forwarded-Proto=https` being forwarded correctly.

The CSRF request-token cookie remains readable by the frontend and also becomes
`Secure` for HTTPS requests.

## CORS

CORS remains explicit and credentialed. Production allowed origins are:

- `https://fitlead.space`
- `https://www.fitlead.space`

Do not use `AllowAnyOrigin()` with credentials.
