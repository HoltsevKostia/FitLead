const accessTokenCookieName = "fitlead.access_token";
const refreshTokenCookieName = "fitlead.refresh_token";
const legacyRefreshTokenPath = "/auth";

export function getSetCookieHeaders(response: Response): string[] {
  const headersWithGetSetCookie = response.headers as Headers & {
    getSetCookie?: () => string[];
  };

  if (!headersWithGetSetCookie.getSetCookie) {
    throw new Error("getSetCookie is not available in this runtime.");
  }

  return headersWithGetSetCookie.getSetCookie();
}

export function normalizeAuthSetCookieHeader(setCookie: string): string {
  const [cookiePair, ...attributes] = setCookie.split(";");
  const separatorIndex = cookiePair.indexOf("=");

  if (separatorIndex < 0) {
    return setCookie;
  }

  const cookieName = cookiePair.slice(0, separatorIndex).trim();
  if (cookieName !== refreshTokenCookieName) {
    return setCookie;
  }

  let hasPath = false;
  const normalizedAttributes = attributes.map((attribute) => {
    const trimmedAttribute = attribute.trim();

    if (trimmedAttribute.toLowerCase().startsWith("path=")) {
      hasPath = true;
      return " Path=/";
    }

    return attribute;
  });

  if (!hasPath) {
    normalizedAttributes.push(" Path=/");
  }

  return [cookiePair, ...normalizedAttributes].join(";");
}

function isRefreshTokenSetCookie(setCookie: string): boolean {
  const [cookiePair] = setCookie.split(";");
  const separatorIndex = cookiePair.indexOf("=");

  if (separatorIndex < 0) {
    return false;
  }

  return cookiePair.slice(0, separatorIndex).trim() === refreshTokenCookieName;
}

function buildLegacyRefreshTokenDeleteCookie(): string {
  return buildDeleteCookie(refreshTokenCookieName, legacyRefreshTokenPath);
}

function buildDeleteCookie(cookieName: string, path: string): string {
  const attributes = [
    `${cookieName}=`,
    " Expires=Thu, 01 Jan 1970 00:00:00 GMT",
    " Max-Age=0",
    ` Path=${path}`,
    " HttpOnly",
    " SameSite=Lax",
  ];

  if (process.env.NODE_ENV === "production") {
    attributes.push(" Secure");
  }

  return attributes.join(";");
}

export function appendClearAuthCookieHeaders(response: Response) {
  response.headers.append(
    "Set-Cookie",
    buildDeleteCookie(accessTokenCookieName, "/"),
  );
  response.headers.append(
    "Set-Cookie",
    buildDeleteCookie(refreshTokenCookieName, "/"),
  );
  response.headers.append("Set-Cookie", buildLegacyRefreshTokenDeleteCookie());
}

export function appendAuthSetCookieHeaders(
  response: Response,
  setCookieHeaders: string[],
) {
  for (const setCookie of setCookieHeaders) {
    response.headers.append("Set-Cookie", normalizeAuthSetCookieHeader(setCookie));

    if (isRefreshTokenSetCookie(setCookie)) {
      response.headers.append("Set-Cookie", buildLegacyRefreshTokenDeleteCookie());
    }
  }
}
