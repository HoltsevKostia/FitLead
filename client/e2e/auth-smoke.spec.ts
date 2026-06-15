import { expect, test, type Page } from "@playwright/test";

const trainerEmail = "demo.trainer@fitlead.local";
const trainerPassword = "Demo123!";

async function expectLoginForm(page: Page) {
  await expect(page.getByLabel("Електронна пошта")).toBeVisible();
  await expect(page.getByLabel("Пароль", { exact: true })).toBeVisible();
  await expect(page.getByRole("button", { name: "Увійти" })).toBeVisible();
}

async function loginAsTrainer(page: Page) {
  await page.goto("/login");
  await page.getByLabel("Електронна пошта").fill(trainerEmail);
  await page.getByLabel("Пароль", { exact: true }).fill(trainerPassword);
  await page.getByRole("button", { name: "Увійти" }).click();

  await expect(page).toHaveURL(/\/dashboard$/);
  await expect(page.getByRole("button", { name: "Вийти" })).toBeVisible();
}

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
      .filter((name) => name === "fitlead.access_token" || name === "fitlead.refresh_token");

    expect(authCookieNames).toEqual([]);
  });
});
