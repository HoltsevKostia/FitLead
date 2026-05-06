"use client";

import { useState } from "react";

import { mapRevokeInvitationError } from "@/features/invitations/model/error-mapping";
import { invitationsApi } from "@/lib/api/clients/invitations-api";
import { FormAlert } from "@/shared/forms/form-alert";

interface RevokeInvitationButtonProps {
  invitationId: string;
  onRevoked: () => void;
}

export function RevokeInvitationButton({
  invitationId,
  onRevoked,
}: RevokeInvitationButtonProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  async function handleRevoke() {
    setIsSubmitting(true);
    setSubmitError(null);

    try {
      await invitationsApi.revoke(invitationId);
      onRevoked();
    } catch (error) {
      setSubmitError(mapRevokeInvitationError(error));
      setIsSubmitting(false);
    }
  }

  return (
    <div className="space-y-2">
      <button
        type="button"
        onClick={handleRevoke}
        disabled={isSubmitting}
        className="rounded-full border border-rose-200 px-4 py-2 text-sm font-medium text-rose-700 transition hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-70"
      >
        {isSubmitting ? "Відкликаємо..." : "Відкликати"}
      </button>
      <FormAlert message={submitError} />
    </div>
  );
}
