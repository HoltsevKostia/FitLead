import { expect, test } from "@playwright/test";

import { loginAsClient, loginAsTrainer } from "./helpers/auth";

const programTitle = "Smoke Test Program";
const workoutName = "Smoke Test Workout";
const videoReportTitle = "Smoke Test Video Report";

test.describe("product smoke", () => {
  test("trainer opens a pending video report", async ({ page }) => {
    await loginAsTrainer(page);
    await page.goto("/video-reports");

    const reportCard = page.locator("article").filter({
      has: page.getByRole("heading", { name: videoReportTitle }),
    });

    await expect(reportCard).toBeVisible();
    await reportCard.getByRole("link").click();

    await expect(page).toHaveURL(/\/chats\/[^/]+\/reports\/[^/]+$/);
    await expect(
      page.getByRole("heading", { name: videoReportTitle }),
    ).toBeVisible();
  });

  test("client opens an assigned workout and sees logging controls", async ({
    page,
  }) => {
    await loginAsClient(page);
    await page.goto("/client/training-programs");

    const programCard = page.locator("article").filter({
      has: page.getByRole("heading", { name: programTitle }),
    });

    await expect(programCard).toBeVisible();
    await programCard.getByRole("link").click();
    await expect(page.getByRole("heading", { name: programTitle })).toBeVisible();

    const workoutCard = page
      .getByRole("heading", { name: workoutName, exact: true })
      .locator("xpath=ancestor::article[1]");

    await expect(workoutCard).toBeVisible();
    await workoutCard.getByRole("link", { name: "Відкрити" }).click();

    await expect(page.getByRole("heading", { name: workoutName })).toBeVisible();
    await expect(
      page.getByRole("button", { name: "Виконано", exact: true }),
    ).toBeVisible();
    await expect(
      page.getByRole("button", { name: "Пропустити", exact: true }),
    ).toBeVisible();
  });
});
