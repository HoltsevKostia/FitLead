"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

import type { TrainingProgramAssignment } from "@/entities/training-program/model/types";
import { mapTrainingProgramMutationError } from "@/features/training-programs/model/error-mapping";
import { trainingProgramsApi } from "@/lib/api/clients/training-programs-api";
import { FormAlert } from "@/shared/forms/form-alert";

interface TrainingProgramAssignmentListProps {
  programId: string;
  assignments: TrainingProgramAssignment[];
}

const statusLabels: Record<string, string> = {
  Active: "Активний",
  Revoked: "Відкликаний",
  Expired: "Прострочений",
};

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

function AssignmentActions({
  programId,
  assignment,
}: {
  programId: string;
  assignment: TrainingProgramAssignment;
}) {
  const router = useRouter();
  const [isRevoking, setIsRevoking] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const isActive = assignment.status === "Active";

  async function handleRevoke() {
    setIsRevoking(true);
    setError(null);

    try {
      await trainingProgramsApi.revokeAssignment(programId, assignment.assignmentId);
      router.refresh();
    } catch (caughtError) {
      setError(mapTrainingProgramMutationError(caughtError));
    } finally {
      setIsRevoking(false);
    }
  }

  return (
    <div className="space-y-2">
      <button
        type="button"
        onClick={handleRevoke}
        disabled={isRevoking || !isActive}
        className="w-fit rounded-full border border-red-200 px-3 py-2 text-sm font-medium text-red-700 transition hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-60"
      >
        {isRevoking ? "Відкликаємо..." : "Відкликати"}
      </button>
      <FormAlert message={error} />
    </div>
  );
}

export function TrainingProgramAssignmentList({
  programId,
  assignments,
}: TrainingProgramAssignmentListProps) {
  return (
    <section className="space-y-3 rounded-2xl border border-border bg-surface px-5 py-5">
      <div className="flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between">
        <h2 className="text-lg font-semibold text-foreground">Призначені клієнти</h2>
        <span className="text-sm text-muted">{assignments.length} всього</span>
      </div>

      {assignments.length === 0 ? (
        <p className="rounded-xl border border-dashed border-border bg-white/70 px-4 py-5 text-sm text-muted">
          Програму ще не призначено клієнтам.
        </p>
      ) : (
        <div className="space-y-3">
          {assignments.map((assignment) => (
            <article
              key={assignment.assignmentId}
              className="rounded-xl border border-border bg-white px-4 py-4"
            >
              <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                <div className="min-w-0 space-y-2">
                  <div className="flex flex-wrap items-center gap-2">
                    <h3 className="text-base font-semibold text-foreground">
                      {assignment.clientName}
                    </h3>
                    <span className="rounded-full border border-border bg-surface px-2 py-1 text-xs text-muted">
                      {statusLabels[assignment.status] ?? assignment.status}
                    </span>
                    <span className="rounded-full border border-border bg-surface px-2 py-1 text-xs text-muted">
                      {assignment.accessSource}
                    </span>
                  </div>
                  <p className="text-sm text-muted">
                    Призначено: {formatDate(assignment.assignedAtUtc)} · Доступ до:{" "}
                    {formatDate(assignment.expiresAtUtc)}
                  </p>
                  {assignment.revokedAtUtc ? (
                    <p className="text-sm text-muted">
                      Відкликано: {formatDate(assignment.revokedAtUtc)}
                    </p>
                  ) : null}
                </div>

                <AssignmentActions programId={programId} assignment={assignment} />
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
