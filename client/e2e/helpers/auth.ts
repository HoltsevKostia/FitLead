import {
  expect,
  type BrowserContext,
  type Cookie,
  type Page,
} from "@playwright/test";

export const accessTokenCookieName = "fitlead.access_token";
export const refreshTokenCookieName = "fitlead.refresh_token";

const trainerEmail = "demo.trainer@fitlead.local";
const trainerPassword = "Demo123!";

export async function expectLoginForm(page: Page) {
  await expect(page.getByLabel("Електронна пошта")).toBeVisible();
  await expect(page.getByLabel("Пароль", { exact: true })).toBeVisible();
  await expect(page.getByRole("button", { name: "Увійти" })).toBeVisible();
}

export async function submitTrainerLogin(page: Page) {
  await page.getByLabel("Електронна пошта").fill(trainerEmail);
  await page.getByLabel("Пароль", { exact: true }).fill(trainerPassword);
  await page.getByRole("button", { name: "Увійти" }).click();
}

export async function loginAsTrainer(page: Page) {
  await page.goto("/login");
  await submitTrainerLogin(page);

  await expect(page).toHaveURL(/\/dashboard$/);
  await expect(page.getByRole("button", { name: "Вийти" })).toBeVisible();
}

export async function getRequiredCookie(
  context: BrowserContext,
  name: string,
): Promise<Cookie> {
  const cookie = (await context.cookies()).find((candidate) => candidate.name === name);

  expect(cookie, `Expected cookie '${name}' to exist`).toBeDefined();
  return cookie!;
}

export async function expectCookieMissing(
  context: BrowserContext,
  name: string,
) {
  const cookie = (await context.cookies()).find((candidate) => candidate.name === name);
  expect(cookie, `Expected cookie '${name}' to be absent`).toBeUndefined();
}
