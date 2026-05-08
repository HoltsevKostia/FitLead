"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

import { authApi } from "@/lib/api/clients/auth-api";

export function LogoutButton() {
  const router = useRouter();
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleLogout() {
    setIsSubmitting(true);

    try {
      await authApi.logout();
      router.replace("/login");
      router.refresh();
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <button
      type="button"
      onClick={handleLogout}
      disabled={isSubmitting}
      className="mt-6 w-full rounded-2xl border border-border px-4 py-3 text-sm font-medium text-foreground transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
    >
      {isSubmitting ? "Виходимо..." : "Вийти"}
    </button>
  );
}
