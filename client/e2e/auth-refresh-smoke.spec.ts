import { expect, test } from "@playwright/test";

import {
  accessTokenCookieName,
  expectCookieMissing,
  expectLoginForm,
  getRequiredCookie,
  loginAsTrainer,
  refreshTokenCookieName,
  submitTrainerLogin,
} from "./helpers/auth";

test.describe("authentication refresh smoke", () => {
  test("refreshes the session when only the access token is missing", async ({
    page,
    context,
  }) => {
    await loginAsTrainer(page);

    const refreshCookieBefore = await getRequiredCookie(
      context,
      refreshTokenCookieName,
    );

    await context.clearCookies({ name: accessTokenCookieName });

    await expectCookieMissing(context, accessTokenCookieName);
    expect(
      (await getRequiredCookie(context, refreshTokenCookieName)).value,
    ).toBe(refreshCookieBefore.value);

    await page.goto("/workouts");

    await expect(page).toHaveURL(/\/workouts$/);
    await expect(page.getByRole("button", { name: "Вийти" })).toBeVisible();

    await getRequiredCookie(context, accessTokenCookieName);
    const rotatedRefreshCookie = await getRequiredCookie(
      context,
      refreshTokenCookieName,
    );
    expect(rotatedRefreshCookie.value).not.toBe(refreshCookieBefore.value);
  });

  test("redirects to login when both auth cookies are missing", async ({
    page,
    context,
  }) => {
    await loginAsTrainer(page);

    await context.clearCookies({ name: accessTokenCookieName });
    await context.clearCookies({ name: refreshTokenCookieName });

    await page.goto("/chats");

    await expect(page).toHaveURL(/\/login\?next=%2Fchats$/);
    await expectLoginForm(page);
  });

  test("clears an invalid refresh cookie and redirects to login", async ({
    page,
    context,
  }) => {
    await loginAsTrainer(page);

    const refreshCookie = await getRequiredCookie(
      context,
      refreshTokenCookieName,
    );

    await context.clearCookies({ name: accessTokenCookieName });
    await context.clearCookies({ name: refreshTokenCookieName });
    await context.addCookies([
      {
        ...refreshCookie,
        value: "invalid-refresh-token",
      },
    ]);

    await page.goto("/training-programs");

    await expect(page).toHaveURL(
      /\/login\?next=%2Ftraining-programs$/,
    );
    await expectLoginForm(page);
    await expectCookieMissing(context, accessTokenCookieName);
    await expectCookieMissing(context, refreshTokenCookieName);
  });

  for (const unsafeNext of [
    "https://example.com/account",
    "//example.com/account",
  ]) {
    test(`rejects unsafe next destination: ${unsafeNext}`, async ({ page }) => {
      await page.goto(`/login?next=${encodeURIComponent(unsafeNext)}`);
      await submitTrainerLogin(page);

      await expect(page).toHaveURL(/\/dashboard$/);
      await expect(page.getByRole("button", { name: "Вийти" })).toBeVisible();
    });
  }
});
