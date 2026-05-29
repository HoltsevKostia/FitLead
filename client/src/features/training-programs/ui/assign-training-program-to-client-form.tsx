"use client";

import { useRouter } from "next/navigation";
import { type SubmitEvent, useMemo, useState } from "react";

import type { TrainerClient } from "@/entities/user/model/types";
import { mapTrainingProgramMutationError } from "@/features/training-programs/model/error-mapping";
import { trainingProgramsApi } from "@/lib/api/clients/training-programs-api";
import { fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";
import { FormAlert } from "@/shared/forms/form-alert";

interface AssignTrainingProgramToClientFormProps {
  programId: string;
  clients: TrainerClient[];
  initialClientId?: string;
}

function getFilteredClients(clients: TrainerClient[], search: string): TrainerClient[] {
  const normalizedSearch = search.trim().toLowerCase();

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

function toExpiresAtUtc(expiresAtDate: string): string | null {
  if (!expiresAtDate) {
    return null;
  }

  return new Date(`${expiresAtDate}T23:59:59.999Z`).toISOString();
}

export function AssignTrainingProgramToClientForm({
  programId,
  clients,
  initialClientId,
}: AssignTrainingProgramToClientFormProps) {
  const router = useRouter();
  const hasInitialClient = Boolean(
    initialClientId && clients.some((client) => client.clientId === initialClientId),
  );
  const [isOpen, setIsOpen] = useState(hasInitialClient);
  const [selectedClientId, setSelectedClientId] = useState(
    hasInitialClient ? initialClientId! : "",
  );
  const [search, setSearch] = useState("");
  const [expiresAtDate, setExpiresAtDate] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const visibleClients = useMemo(
    () => getFilteredClients(clients, search),
    [clients, search],
  );
  const selectedClient =
    clients.find((client) => client.clientId === selectedClientId) ?? null;

  function closeForm() {
    setIsOpen(false);
    setSelectedClientId("");
    setSearch("");
    setExpiresAtDate("");
    setFormError(null);
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedClient) {
      setFormError("Виберіть клієнта.");
      return;
    }

    setIsSubmitting(true);
    setFormError(null);
    setSuccessMessage(null);

    try {
      await trainingProgramsApi.assignToClient(programId, {
        clientId: selectedClient.clientId,
        expiresAtUtc: toExpiresAtUtc(expiresAtDate),
      });
      setSuccessMessage(`Програму призначено клієнту ${selectedClient.fullName}.`);
      closeForm();
      router.refresh();
    } catch (error) {
      setFormError(mapTrainingProgramMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="space-y-3">
      <button
        type="button"
        onClick={() => (isOpen ? closeForm() : setIsOpen(true))}
        disabled={isSubmitting}
        className="rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
      >
        {isOpen ? "Закрити призначення" : "Призначити клієнту"}
      </button>

      {successMessage ? (
        <p className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
          {successMessage}
        </p>
      ) : null}

      {isOpen ? (
        <form
          onSubmit={handleSubmit}
          className="space-y-4 rounded-2xl border border-border bg-white px-5 py-5"
        >
          {clients.length === 0 ? (
            <p className="rounded-xl border border-dashed border-border bg-surface px-4 py-4 text-sm text-muted">
              У вас ще немає клієнтів для призначення програми.
            </p>
          ) : (
            <>
              <label className={fieldLabelClassName} htmlFor="assign-program-client-search">
                Клієнт
              </label>
              <input
                id="assign-program-client-search"
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                disabled={isSubmitting}
                placeholder="Пошук клієнта"
                className={fieldInputClassName}
              />

              <div className="max-h-72 space-y-2 overflow-y-auto rounded-2xl border border-border bg-surface p-2">
                {visibleClients.length === 0 ? (
                  <p className="px-3 py-4 text-sm text-muted">Клієнтів не знайдено.</p>
                ) : null}

                {visibleClients.map((client) => {
                  const isSelected = client.clientId === selectedClientId;

                  return (
                    <button
                      key={client.clientId}
                      type="button"
                      aria-pressed={isSelected}
                      onClick={() => setSelectedClientId(client.clientId)}
                      disabled={isSubmitting}
                      className={`w-full rounded-xl border px-4 py-3 text-left transition ${
                        isSelected
                          ? "border-accent bg-emerald-50"
                          : "border-border bg-white hover:border-accent"
                      }`}
                    >
                      <span className="block truncate text-sm font-semibold text-foreground">
                        {client.fullName}
                      </span>
                      <span className="mt-1 block truncate text-xs text-muted">
                        {client.email}
                      </span>
                    </button>
                  );
                })}
              </div>

              <div className="space-y-2">
                <label className={fieldLabelClassName} htmlFor="assign-program-expires-at">
                  Дата завершення доступу
                </label>
                <input
                  id="assign-program-expires-at"
                  type="date"
                  value={expiresAtDate}
                  onChange={(event) => setExpiresAtDate(event.target.value)}
                  disabled={isSubmitting}
                  className={fieldInputClassName}
                />
              </div>

              {selectedClient ? (
                <p className="text-sm text-muted">
                  Обрано:{" "}
                  <span className="font-medium text-foreground">
                    {selectedClient.fullName}
                  </span>
                </p>
              ) : null}
            </>
          )}

          <FormAlert message={formError} />

          <div className="flex flex-col gap-3 sm:flex-row">
            <button
              type="submit"
              disabled={isSubmitting || clients.length === 0 || !selectedClient}
              className="rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
            >
              {isSubmitting ? "Призначаємо..." : "Призначити"}
            </button>
            <button
              type="button"
              onClick={closeForm}
              disabled={isSubmitting}
              className="rounded-full border border-border px-5 py-2 text-sm font-medium transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
            >
              Скасувати
            </button>
          </div>
        </form>
      ) : null}
    </div>
  );
}
