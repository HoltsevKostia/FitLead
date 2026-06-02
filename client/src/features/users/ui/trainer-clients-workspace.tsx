"use client";

import Link from "next/link";
import { useMemo, useState } from "react";

import type {
  TrainerClientOverview,
  TrainerClientProgramAccess,
} from "@/entities/user/model/types";
import { OpenChatButton } from "@/features/chats/ui/open-chat-button";

interface TrainerClientsWorkspaceProps {
  clients: TrainerClientOverview[];
  loadError?: string | null;
}

function normalize(value: string): string {
  return value.trim().toLowerCase();
}

function formatDate(value: string | null): string {
  if (!value) {
    return "Безстроково";
  }

  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(value));
}

function ProgramAccessBadge({ program }: { program: TrainerClientProgramAccess }) {
  return (
    <Link
      href={`/training-programs/${program.programId}`}
      className="inline-flex max-w-full flex-wrap items-center gap-2 rounded-full border border-border bg-surface px-3 py-2 text-sm font-medium text-foreground transition hover:bg-surface-strong"
    >
      <span className="min-w-0 truncate">{program.programTitle}</span>
      <span className="text-muted">до {formatDate(program.expiresAtUtc)}</span>
    </Link>
  );
}

function filterClients(
  clients: TrainerClientOverview[],
  search: string,
): TrainerClientOverview[] {
  const normalizedSearch = normalize(search);

  if (!normalizedSearch) {
    return clients;
  }

  return clients.filter((client) => {
    return (
      client.fullName.toLowerCase().includes(normalizedSearch) ||
      client.email.toLowerCase().includes(normalizedSearch)
    );
  });
}

export function TrainerClientsWorkspace({
  clients,
  loadError,
}: TrainerClientsWorkspaceProps) {
  const [search, setSearch] = useState("");
  const visibleClients = useMemo(
    () => filterClients(clients, search),
    [clients, search],
  );

  return (
    <section className="space-y-6">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div className="space-y-3">
          <h1 className="text-3xl font-semibold tracking-tight">Клієнти</h1>
          <p className="max-w-3xl text-muted">
            Список клієнтів тренера та їхні активні доступи до тренувальних програм.
          </p>
        </div>

        <div className="w-full lg:max-w-xs">
          <label className="mb-2 block text-sm font-medium text-foreground" htmlFor="client-search">
            Пошук
          </label>
          <input
            id="client-search"
            type="search"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Ім'я або email"
            className="w-full rounded-2xl border border-border bg-white px-4 py-3 outline-none transition focus:border-accent"
          />
        </div>
      </div>

      {loadError ? (
        <div className="rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-800">
          {loadError}
        </div>
      ) : null}

      {!loadError && clients.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border px-6 py-8 text-center">
          <p className="text-lg font-medium text-foreground">Клієнтів ще немає.</p>
          <p className="mt-2 text-sm text-muted">
            Після прийняття запрошення клієнт з&apos;явиться у цьому списку.
          </p>
        </div>
      ) : null}

      {!loadError && clients.length > 0 && visibleClients.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border px-6 py-8 text-center">
          <p className="text-lg font-medium text-foreground">Нічого не знайдено.</p>
          <p className="mt-2 text-sm text-muted">Спробуйте змінити пошуковий запит.</p>
        </div>
      ) : null}

      {visibleClients.length > 0 ? (
        <div className="grid gap-4">
          {visibleClients.map((client) => (
            <article
              key={client.clientId}
              className="rounded-2xl border border-border bg-white px-5 py-5"
            >
              <div className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_minmax(260px,1.2fr)] lg:items-start">
                <div className="min-w-0 space-y-3">
                  <div>
                    <h2 className="text-xl font-semibold text-foreground">
                      <Link
                        href={`/clients/${client.clientId}`}
                        className="hover:text-accent"
                      >
                        {client.fullName}
                      </Link>
                    </h2>
                    <p className="mt-2 break-words text-sm text-muted">{client.email}</p>
                  </div>
                  <div className="flex flex-col gap-2 sm:flex-row sm:items-start">
                    <Link
                      href={`/clients/${client.clientId}`}
                      className="w-fit rounded-full bg-accent px-4 py-2 text-sm font-medium text-white transition hover:bg-accent-strong"
                    >
                      Сторінка клієнта
                    </Link>
                    <OpenChatButton targetId={client.clientId} targetType="client" />
                  </div>
                </div>

                <div className="space-y-3">
                  <p className="text-sm font-medium text-foreground">Активні програми</p>
                  {client.activePrograms.length === 0 ? (
                    <p className="rounded-xl border border-dashed border-border bg-surface px-4 py-4 text-sm text-muted">
                      Програму не призначено
                    </p>
                  ) : (
                    <div className="flex flex-wrap gap-2">
                      {client.activePrograms.map((program) => (
                        <ProgramAccessBadge
                          key={program.assignmentId}
                          program={program}
                        />
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </article>
          ))}
        </div>
      ) : null}
    </section>
  );
}
