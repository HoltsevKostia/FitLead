"use client";

import { type SubmitEvent, useState } from "react";
import { useRouter } from "next/navigation";

import { mapTrainingProgramMutationError } from "@/features/training-programs/model/error-mapping";
import { trainingProgramsApi } from "@/lib/api/clients/training-programs-api";
import { fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";
import { FormAlert } from "@/shared/forms/form-alert";

interface CreateTrainingProgramFormProps {
  onCreated: () => void;
  onCancel: () => void;
}

const defaultWeeksCount = 4;
const defaultDaysPerWeek = 7;

export function CreateTrainingProgramForm({
  onCreated,
  onCancel,
}: CreateTrainingProgramFormProps) {
  const router = useRouter();
  const [title, setTitle] = useState("");
  const [weeksCount, setWeeksCount] = useState(defaultWeeksCount);
  const [daysPerWeek, setDaysPerWeek] = useState(defaultDaysPerWeek);
  const [titleError, setTitleError] = useState<string | null>(null);
  const [weeksCountError, setWeeksCountError] = useState<string | null>(null);
  const [daysPerWeekError, setDaysPerWeekError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function resetForm() {
    setTitle("");
    setWeeksCount(defaultWeeksCount);
    setDaysPerWeek(defaultDaysPerWeek);
    setTitleError(null);
    setWeeksCountError(null);
    setDaysPerWeekError(null);
    setSubmitError(null);
  }

  function handleCancel() {
    resetForm();
    onCancel();
  }

  function validate(): boolean {
    const trimmedTitle = title.trim();
    let isValid = true;

    if (!trimmedTitle) {
      setTitleError("Вкажіть назву програми.");
      isValid = false;
    }

    if (weeksCount < 1 || weeksCount > 24) {
      setWeeksCountError("Кількість тижнів має бути від 1 до 24.");
      isValid = false;
    }

    if (daysPerWeek < 1 || daysPerWeek > 7) {
      setDaysPerWeekError("Кількість днів у тижні має бути від 1 до 7.");
      isValid = false;
    }

    return isValid;
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    setTitleError(null);
    setWeeksCountError(null);
    setDaysPerWeekError(null);
    setSubmitError(null);

    if (!validate()) {
      return;
    }

    setIsSubmitting(true);

    try {
      await trainingProgramsApi.createTrainingProgram({
        title: title.trim(),
        weeksCount,
        daysPerWeek,
      });
      resetForm();
      router.refresh();
      onCreated();
    } catch (error) {
      setSubmitError(mapTrainingProgramMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-4 rounded-2xl border border-border bg-white px-5 py-5"
    >
      <div className="grid gap-4 md:grid-cols-[minmax(0,1fr)_160px_160px]">
        <div className="space-y-2">
          <label className={fieldLabelClassName} htmlFor="create-program-title">
            Назва
          </label>
          <input
            id="create-program-title"
            value={title}
            onChange={(event) => {
              setTitle(event.target.value);
              if (titleError) {
                setTitleError(null);
              }
            }}
            disabled={isSubmitting}
            required
            maxLength={200}
            aria-invalid={titleError ? "true" : "false"}
            aria-describedby={titleError ? "create-program-title-error" : undefined}
            className={fieldInputClassName}
          />
          {titleError ? (
            <p id="create-program-title-error" className="text-sm text-red-700">
              {titleError}
            </p>
          ) : null}
        </div>

        <div className="space-y-2">
          <label className={fieldLabelClassName} htmlFor="create-program-weeks">
            Кількість тижнів
          </label>
          <input
            id="create-program-weeks"
            type="number"
            min={1}
            max={24}
            value={weeksCount}
            onChange={(event) => {
              setWeeksCount(Number(event.target.value));
              if (weeksCountError) {
                setWeeksCountError(null);
              }
            }}
            disabled={isSubmitting}
            required
            aria-invalid={weeksCountError ? "true" : "false"}
            aria-describedby={weeksCountError ? "create-program-weeks-error" : undefined}
            className={fieldInputClassName}
          />
          {weeksCountError ? (
            <p id="create-program-weeks-error" className="text-sm text-red-700">
              {weeksCountError}
            </p>
          ) : null}
        </div>

        <div className="space-y-2">
          <label className={fieldLabelClassName} htmlFor="create-program-days">
            Днів у тижні
          </label>
          <input
            id="create-program-days"
            type="number"
            min={1}
            max={7}
            value={daysPerWeek}
            onChange={(event) => {
              setDaysPerWeek(Number(event.target.value));
              if (daysPerWeekError) {
                setDaysPerWeekError(null);
              }
            }}
            disabled={isSubmitting}
            required
            aria-invalid={daysPerWeekError ? "true" : "false"}
            aria-describedby={daysPerWeekError ? "create-program-days-error" : undefined}
            className={fieldInputClassName}
          />
          {daysPerWeekError ? (
            <p id="create-program-days-error" className="text-sm text-red-700">
              {daysPerWeekError}
            </p>
          ) : null}
        </div>
      </div>

      <FormAlert message={submitError} />

      <div className="flex flex-col gap-3 sm:flex-row">
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-full bg-accent px-5 py-2 text-sm font-medium text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isSubmitting ? "Створюємо..." : "Створити"}
        </button>
        <button
          type="button"
          onClick={handleCancel}
          disabled={isSubmitting}
          className="rounded-full border border-border px-5 py-2 text-sm font-medium transition hover:bg-surface-strong disabled:cursor-not-allowed disabled:opacity-70"
        >
          Скасувати
        </button>
      </div>
    </form>
  );
}
