import type { ReactNode } from "react";
import Link from "next/link";

import type { InvitationPreview } from "@/entities/invitation/model/types";
import { getCurrentUser } from "@/features/auth/server/get-current-user";
import { AcceptInvitationButton } from "@/features/invitations/ui/accept-invitation-button";
import { InvitationPreviewCard } from "@/features/invitations/ui/invitation-preview-card";
import { invitationsApi } from "@/lib/api/clients/invitations-api";
import { isApiError } from "@/lib/api/api-error";
import { buildAuthHref } from "@/shared/utils/build-auth-href";

interface InvitePageProps {
  params: Promise<{
    token: string;
  }>;
}

function getInvitationMessage(preview: InvitationPreview): string {
  switch (preview.status) {
    case "Pending":
      return "Запрошення активне. Увійди як клієнт або прийми його одразу, якщо вже авторизований.";
    case "Accepted":
      return "Це запрошення вже використано. Якщо ти очікував інший стан, звернися до тренера.";
    case "Expired":
      return "Строк дії цього запрошення завершився. Попроси тренера створити нове.";
    case "Revoked":
      return "Тренер відкликав це запрошення. Якщо воно потрібне тобі зараз, попроси надіслати нове.";
  }
}

function InvalidInvitationState() {
  return (
    <section className="card px-6 py-8 sm:px-8">
      <div className="space-y-4">
        <p className="text-sm uppercase tracking-[0.2em] text-muted">FitLead Invitation</p>
        <h1 className="text-3xl font-semibold tracking-tight">Запрошення не знайдено</h1>
        <p className="max-w-2xl text-base leading-7 text-muted">
          Це посилання недійсне або більше не існує. Перевір його ще раз або попроси тренера
          надіслати нове.
        </p>
        <Link
          href="/login"
          className="inline-flex rounded-full border border-border px-5 py-3 text-sm font-medium transition hover:bg-surface-strong"
        >
          Увійти
        </Link>
      </div>
    </section>
  );
}

export default async function InvitePage({ params }: InvitePageProps) {
  const { token } = await params;
  const nextHref = `/invite/${token}`;

  let preview: InvitationPreview | null = null;

  try {
    preview = await invitationsApi.getPreview(token);
  } catch (error) {
    if (isApiError(error) && error.status === 404) {
      return (
        <div className="app-shell">
          <div className="container py-12 md:py-20">
            <InvalidInvitationState />
          </div>
        </div>
      );
    }

    throw error;
  }

  const currentUser = await getCurrentUser();
  const loginHref = buildAuthHref("/login", nextHref);
  const registerHref = buildAuthHref("/register", nextHref);

  let actions: ReactNode = null;

  if (preview.isJoinable) {
    if (!currentUser) {
      actions = (
        <div className="flex flex-col gap-3 sm:flex-row">
          <Link
            href={loginHref}
            className="rounded-full bg-accent px-5 py-3 text-center text-sm font-medium text-white transition hover:bg-accent-strong"
          >
            Увійти, щоб приєднатися
          </Link>
          <Link
            href={registerHref}
            className="rounded-full border border-border px-5 py-3 text-center text-sm font-medium transition hover:bg-surface-strong"
          >
            Створити акаунт
          </Link>
        </div>
      );
    } else if (currentUser.role === "Client") {
      actions = <AcceptInvitationButton token={token} />;
    } else {
      actions = (
        <p className="rounded-2xl border border-border bg-surface-strong/60 px-4 py-3 text-sm text-muted">
          Це запрошення може прийняти лише користувач із роллю клієнта.
        </p>
      );
    }
  }

  return (
    <div className="app-shell">
      <div className="container py-12 md:py-20">
        <InvitationPreviewCard
          trainerName={preview.trainer.fullName}
          status={preview.status}
          expiresAtUtc={preview.expiresAtUtc}
          message={getInvitationMessage(preview)}
          actions={actions}
        />
      </div>
    </div>
  );
}
