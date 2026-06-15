import { expect, test } from "@playwright/test";

import {
  accessTokenCookieName,
  expectLoginForm,
  loginAsTrainer,
  refreshTokenCookieName,
} from "./helpers/auth";

test.describe("authentication smoke", () => {
  test("redirects an unauthenticated dashboard request to login", async ({ page }) => {
    await page.goto("/dashboard");

    await expect(page).toHaveURL(/\/login\?next=%2Fdashboard$/);
    await expectLoginForm(page);
  });

  test("renders the login form", async ({ page }) => {
    await page.goto("/login");

    await expectLoginForm(page);
  });

  test("renders the register form", async ({ page }) => {
    await page.goto("/register");

    await expect(page.getByLabel("Повне ім’я")).toBeVisible();
    await expect(page.getByLabel("Електронна пошта")).toBeVisible();
    await expect(page.getByLabel("Пароль", { exact: true })).toBeVisible();
    await expect(page.getByRole("radio", { name: /Тренер/ })).toBeVisible();
    await expect(page.getByRole("radio", { name: /Клієнт/ })).toBeVisible();
    await expect(page.getByRole("button", { name: "Створити акаунт" })).toBeVisible();
  });

  test("logs in a seeded trainer and opens the dashboard", async ({ page }) => {
    await loginAsTrainer(page);

    await expect(page.getByRole("heading", { name: "Панель" })).toBeVisible();
  });

  test("logs out and clears the authenticated session", async ({ page, context }) => {
    await loginAsTrainer(page);

    await page.getByRole("button", { name: "Вийти" }).click();

    await expect(page).toHaveURL(/\/login$/);
    await expectLoginForm(page);

    const authCookieNames = (await context.cookies())
      .map((cookie) => cookie.name)
      .filter(
        (name) =>
          name === accessTokenCookieName ||
          name === refreshTokenCookieName,
      );

    expect(authCookieNames).toEqual([]);
  });
});
