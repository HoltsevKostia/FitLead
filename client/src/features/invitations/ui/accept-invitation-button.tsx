"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";

import { mapAcceptInvitationError } from "@/features/invitations/model/error-mapping";
import { invitationsApi } from "@/lib/api/clients/invitations-api";
import { FormAlert } from "@/shared/forms/form-alert";

interface AcceptInvitationButtonProps {
  token: string;
}

export function AcceptInvitationButton({ token }: AcceptInvitationButtonProps) {
  const router = useRouter();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  async function handleAccept() {
    setIsSubmitting(true);
    setSubmitError(null);

    try {
      await invitationsApi.accept(token);
      router.replace("/dashboard");
      router.refresh();
    } catch (error) {
      setSubmitError(mapAcceptInvitationError(error));
      setIsSubmitting(false);
    }
  }

  return (
    <div className="space-y-3">
      <button
        type="button"
        onClick={handleAccept}
        disabled={isSubmitting}
        className="w-full rounded-full bg-accent px-5 py-3 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70 sm:w-auto"
      >
        {isSubmitting ? "Приєднуємо..." : "Прийняти запрошення"}
      </button>

      <FormAlert message={submitError} />
    </div>
  );
}
