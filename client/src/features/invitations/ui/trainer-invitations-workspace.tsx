"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";

import type {
  CreateInvitationResult,
  TrainerInvitation,
} from "@/entities/invitation/model/types";
import { mapCreateInvitationError } from "@/features/invitations/model/error-mapping";
import {
  formatInvitationDate,
  getInvitationDisplayStatus,
  invitationDisplayStatusClasses,
  invitationDisplayStatusLabels,
  isInvitationRevokable,
  sortTrainerInvitations,
} from "@/features/invitations/model/trainer-invitations";
import { invitationsApi } from "@/lib/api/clients/invitations-api";
import { FormAlert } from "@/shared/forms/form-alert";

import { CopyInviteLinkButton } from "./copy-invite-link-button";
import { RevokeInvitationButton } from "./revoke-invitation-button";

interface TrainerInvitationsWorkspaceProps {
  invitations: TrainerInvitation[];
  loadError?: string | null;
}

function CreatedInvitationNotice({
  invitation,
}: {
  invitation: CreateInvitationResult;
}) {
  return (
    <section className="rounded-3xl border border-emerald-200 bg-emerald-50 px-5 py-5">
      <div className="space-y-4">
        <div className="space-y-1">
          <p className="text-sm uppercase tracking-[0.16em] text-emerald-700">
            Нове запрошення
          </p>
          <h2 className="text-xl font-semibold text-foreground">
            Посилання готове до надсилання клієнту
          </h2>
          <p className="text-sm text-muted">
            Запрошення дійсне до {formatInvitationDate(invitation.expiresAtUtc)}.
          </p>
        </div>

        <div className="space-y-2">
          <label className="text-sm font-medium text-foreground" htmlFor="created-invite-url">
            Invite link
          </label>
          <input
            id="created-invite-url"
            value={invitation.inviteUrl}
            readOnly
            className="w-full rounded-2xl border border-border bg-white px-4 py-3 text-sm text-foreground"
          />
          <p className="text-sm text-muted">
            Скопіюй або збережи це посилання зараз. Після переходу на іншу сторінку або
            перезавантаження цей блок зникне, бо система не зберігає raw invite token для
            повторного показу.
          </p>
        </div>

        <CopyInviteLinkButton inviteUrl={invitation.inviteUrl} />
      </div>
    </section>
  );
}

export function TrainerInvitationsWorkspace({
  invitations,
  loadError,
}: TrainerInvitationsWorkspaceProps) {
  const router = useRouter();
  const [createdInvitation, setCreatedInvitation] = useState<CreateInvitationResult | null>(null);
  const [createError, setCreateError] = useState<string | null>(null);
  const [isCreating, setIsCreating] = useState(false);

  const sortedInvitations = sortTrainerInvitations(invitations);

  async function handleCreate(expiresInDays: 7 | 14) {
    setIsCreating(true);
    setCreateError(null);

    try {
      const result = await invitationsApi.create({ expiresInDays });
      setCreatedInvitation(result);
      router.refresh();
    } catch (error) {
      setCreateError(mapCreateInvitationError(error));
      setIsCreating(false);
      return;
    }

    setIsCreating(false);
  }

  function handleRevoked() {
    router.refresh();
  }

  return (
    <section className="space-y-6">
      <div className="space-y-3">
        <h1 className="text-3xl font-semibold tracking-tight">Запрошення</h1>
        <p className="max-w-3xl text-muted">
          Створюй одноразові посилання для клієнтів на 7 або 14 днів. Після прийняття
          запрошення стає недоступним для повторного використання.
        </p>
      </div>

      <section className="card space-y-4 p-6">
        <div className="space-y-1">
          <h2 className="text-xl font-semibold">Створити нове запрошення</h2>
          <p className="text-sm text-muted">
            Вибери строк дії і надішли клієнту готове посилання.
          </p>
        </div>

        <div className="flex flex-col gap-3 sm:flex-row">
          <button
            type="button"
            onClick={() => handleCreate(7)}
            disabled={isCreating}
            className="rounded-full bg-accent px-5 py-3 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
          >
            {isCreating ? "Створюємо..." : "Створити на 7 днів"}
          </button>
          <button
            type="button"
            onClick={() => handleCreate(14)}
            disabled={isCreating}
            className="rounded-full border border-border px-5 py-3 text-sm font-medium transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
          >
            Створити на 14 днів
          </button>
        </div>

        <FormAlert message={createError} />
      </section>

      {createdInvitation ? <CreatedInvitationNotice invitation={createdInvitation} /> : null}

      <section className="card space-y-5 p-6">
        <div className="space-y-1">
          <h2 className="text-xl font-semibold">Мої запрошення</h2>
          <p className="text-sm text-muted">
            Тут видно актуальний стан кожного створеного посилання.
          </p>
        </div>

        {loadError ? <FormAlert message={loadError} /> : null}

        {!loadError && sortedInvitations.length === 0 ? (
          <div className="rounded-3xl border border-dashed border-border px-6 py-8 text-center">
            <p className="text-lg font-medium text-foreground">
              У вас ще немає створених запрошень.
            </p>
            <p className="mt-2 text-sm text-muted">
              Створіть інвайт на 7 або 14 днів, щоб запросити нового клієнта.
            </p>
          </div>
        ) : null}

        {sortedInvitations.length > 0 ? (
          <div className="space-y-4">
            {sortedInvitations.map((invitation) => {
              const displayStatus = getInvitationDisplayStatus(invitation);

              return (
                <article
                  key={invitation.id}
                  className="rounded-3xl border border-border bg-white px-5 py-5"
                >
                  <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                    <div className="space-y-4">
                      <div className="flex flex-wrap items-center gap-3">
                        <span
                          className={`inline-flex rounded-full border px-3 py-1 text-sm font-medium ${invitationDisplayStatusClasses[displayStatus]}`}
                        >
                          {invitationDisplayStatusLabels[displayStatus]}
                        </span>
                        <p className="text-sm text-muted">
                          Створено {formatInvitationDate(invitation.createdAtUtc)}
                        </p>
                      </div>

                      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
                        <div className="space-y-1">
                          <p className="text-sm uppercase tracking-[0.14em] text-muted">
                            Дійсне до
                          </p>
                          <p className="font-medium text-foreground">
                            {formatInvitationDate(invitation.expiresAtUtc)}
                          </p>
                        </div>

                        <div className="space-y-1">
                          <p className="text-sm uppercase tracking-[0.14em] text-muted">
                            Статус
                          </p>
                          <p className="font-medium text-foreground">
                            {invitationDisplayStatusLabels[displayStatus]}
                          </p>
                        </div>

                        <div className="space-y-1">
                          <p className="text-sm uppercase tracking-[0.14em] text-muted">
                            Прийнято
                          </p>
                          <p className="font-medium text-foreground">
                            {invitation.acceptedAtUtc
                              ? formatInvitationDate(invitation.acceptedAtUtc)
                              : "Ще ні"}
                          </p>
                        </div>
                      </div>
                    </div>

                    {isInvitationRevokable(invitation) ? (
                      <RevokeInvitationButton
                        invitationId={invitation.id}
                        onRevoked={handleRevoked}
                      />
                    ) : null}
                  </div>
                </article>
              );
            })}
          </div>
        ) : null}
      </section>
    </section>
  );
}
