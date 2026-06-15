import { expect, test } from "@playwright/test";

import { loginAsTrainer } from "./helpers/auth";

test("trainer connects to the seeded chat through SignalR", async ({ page }) => {
  await loginAsTrainer(page);
  await page.goto("/chats");

  const chatCard = page
    .getByRole("heading", { name: "Demo Client", exact: true })
    .locator("xpath=ancestor::article[1]");

  await expect(chatCard).toBeVisible();
  await chatCard.getByRole("link", { name: "Відкрити" }).click();

  await expect(page).toHaveURL(/\/chats\/[^/]+$/);
  await expect(
    page.getByLabel(/Real-time .* активне/),
  ).toBeVisible();
});
