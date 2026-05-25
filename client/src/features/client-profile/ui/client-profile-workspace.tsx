"use client";

import { type FormEvent, useState } from "react";

import type {
  ClientExperienceLevel,
  ClientProfile,
  UpdateClientProfileRequest,
} from "@/entities/client-profile/model/types";
import { mapClientProfileMutationError } from "@/features/client-profile/model/error-mapping";
import { BodyMetricsTab } from "@/features/client-profile/ui/body-metrics-tab";
import { clientProfileApi } from "@/lib/api/clients/client-profile-api";
import { FormAlert } from "@/shared/forms/form-alert";
import {
  fieldErrorClassName,
  fieldInputClassName,
  fieldLabelClassName,
} from "@/shared/forms/field-styles";

type ClientProfileTab = "profile" | "metrics" | "photos";

interface ClientProfileWorkspaceProps {
  profile: ClientProfile | null;
  loadError: string | null;
}

interface ClientProfileFormValues {
  goal: string;
  experienceLevel: "" | ClientExperienceLevel;
  heightCm: string;
  limitations: string;
  trainingPreferences: string;
  additionalInfo: string;
}

type ClientProfileFieldErrors = Partial<Record<keyof ClientProfileFormValues, string>>;

const tabs: Array<{ id: ClientProfileTab; label: string }> = [
  { id: "profile", label: "Профіль" },
  { id: "metrics", label: "Метрики" },
  { id: "photos", label: "Фото прогресу" },
];

const experienceLevelOptions: Array<{ value: ClientExperienceLevel; label: string }> = [
  { value: "Beginner", label: "Початковий" },
  { value: "Intermediate", label: "Середній" },
  { value: "Advanced", label: "Просунутий" },
];

function getInitialValues(profile: ClientProfile | null): ClientProfileFormValues {
  return {
    goal: profile?.goal ?? "",
    experienceLevel: profile?.experienceLevel ?? "",
    heightCm: profile?.heightCm?.toString() ?? "",
    limitations: profile?.limitations ?? "",
    trainingPreferences: profile?.trainingPreferences ?? "",
    additionalInfo: profile?.additionalInfo ?? "",
  };
}

function normalizeOptionalText(value: string): string | null {
  const trimmedValue = value.trim();
  return trimmedValue ? trimmedValue : null;
}

function parseHeightCm(value: string): number | null {
  const trimmedValue = value.trim();
  if (!trimmedValue) {
    return null;
  }

  return Number(trimmedValue);
}

function parseValidHeightCm(value: string): number | null {
  const parsedHeight = parseHeightCm(value);

  return Number.isInteger(parsedHeight) ? parsedHeight : null;
}

function validateForm(values: ClientProfileFormValues): ClientProfileFieldErrors {
  const errors: ClientProfileFieldErrors = {};
  const parsedHeight = parseHeightCm(values.heightCm);

  if (
    values.heightCm.trim() &&
    (!Number.isInteger(parsedHeight) ||
      parsedHeight === null ||
      parsedHeight < 50 ||
      parsedHeight > 300)
  ) {
    errors.heightCm = "Зріст має бути цілим числом від 50 до 300.";
  }

  if (values.goal.trim().length > 500) {
    errors.goal = "Ціль має бути не довшою за 500 символів.";
  }

  if (values.limitations.trim().length > 1000) {
    errors.limitations = "Обмеження мають бути не довшими за 1000 символів.";
  }

  if (values.trainingPreferences.trim().length > 1000) {
    errors.trainingPreferences = "Побажання мають бути не довшими за 1000 символів.";
  }

  if (values.additionalInfo.trim().length > 1000) {
    errors.additionalInfo = "Додаткова інформація має бути не довшою за 1000 символів.";
  }

  return errors;
}

function toRequest(values: ClientProfileFormValues): UpdateClientProfileRequest {
  return {
    goal: normalizeOptionalText(values.goal),
    experienceLevel: values.experienceLevel || null,
    heightCm: parseValidHeightCm(values.heightCm),
    limitations: normalizeOptionalText(values.limitations),
    trainingPreferences: normalizeOptionalText(values.trainingPreferences),
    additionalInfo: normalizeOptionalText(values.additionalInfo),
  };
}

export function ClientProfileWorkspace({
  profile,
  loadError,
}: ClientProfileWorkspaceProps) {
  const [activeTab, setActiveTab] = useState<ClientProfileTab>("profile");
  const [values, setValues] = useState<ClientProfileFormValues>(() => getInitialValues(profile));
  const [fieldErrors, setFieldErrors] = useState<ClientProfileFieldErrors>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitSuccess, setSubmitSuccess] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function updateField<TField extends keyof ClientProfileFormValues>(
    field: TField,
    value: ClientProfileFormValues[TField],
  ) {
    setValues((currentValues) => ({
      ...currentValues,
      [field]: value,
    }));

    if (fieldErrors[field]) {
      setFieldErrors((currentErrors) => ({
        ...currentErrors,
        [field]: undefined,
      }));
    }

    if (submitError) {
      setSubmitError(null);
    }

    if (submitSuccess) {
      setSubmitSuccess(null);
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const validationErrors = validateForm(values);
    if (Object.keys(validationErrors).length > 0) {
      setFieldErrors(validationErrors);
      setSubmitSuccess(null);
      return;
    }

    setIsSubmitting(true);
    setFieldErrors({});
    setSubmitError(null);
    setSubmitSuccess(null);

    try {
      const updatedProfile = await clientProfileApi.updateProfile(toRequest(values));
      setValues(getInitialValues(updatedProfile));
      setSubmitSuccess("Профіль збережено.");
    } catch (error) {
      setSubmitError(mapClientProfileMutationError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="space-y-6">
      <h1 className="text-3xl font-semibold tracking-tight">Профіль клієнта</h1>

      <div className="flex max-w-full gap-2 overflow-x-auto rounded-2xl border border-border bg-white p-1">
        {tabs.map((tab) => {
          const isActive = tab.id === activeTab;

          return (
            <button
              key={tab.id}
              type="button"
              onClick={() => setActiveTab(tab.id)}
              className={`shrink-0 rounded-xl px-4 py-2 text-sm font-medium transition ${
                isActive
                  ? "bg-accent text-white"
                  : "text-muted hover:bg-surface-strong hover:text-foreground"
              }`}
            >
              {tab.label}
            </button>
          );
        })}
      </div>

      {activeTab === "profile" ? (
        <form onSubmit={handleSubmit} className="space-y-5">
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2 md:col-span-2">
              <label className={fieldLabelClassName} htmlFor="client-profile-goal">
                Ціль
              </label>
              <textarea
                id="client-profile-goal"
                value={values.goal}
                onChange={(event) => updateField("goal", event.currentTarget.value)}
                disabled={isSubmitting}
                rows={3}
                maxLength={500}
                className={`${fieldInputClassName} resize-y`}
              />
              {fieldErrors.goal ? (
                <p className={fieldErrorClassName}>{fieldErrors.goal}</p>
              ) : null}
            </div>

            <div className="space-y-2">
              <label className={fieldLabelClassName} htmlFor="client-profile-experience-level">
                Рівень підготовки
              </label>
              <select
                id="client-profile-experience-level"
                value={values.experienceLevel}
                onChange={(event) =>
                  updateField(
                    "experienceLevel",
                    event.currentTarget.value as ClientProfileFormValues["experienceLevel"],
                  )
                }
                disabled={isSubmitting}
                className={fieldInputClassName}
              >
                <option value="">Не вказано</option>
                {experienceLevelOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="space-y-2">
              <label className={fieldLabelClassName} htmlFor="client-profile-height">
                Зріст, см
              </label>
              <input
                id="client-profile-height"
                type="number"
                min={50}
                max={300}
                step={1}
                value={values.heightCm}
                onChange={(event) => updateField("heightCm", event.currentTarget.value)}
                disabled={isSubmitting}
                className={fieldInputClassName}
              />
              {fieldErrors.heightCm ? (
                <p className={fieldErrorClassName}>{fieldErrors.heightCm}</p>
              ) : null}
            </div>

            <div className="space-y-2 md:col-span-2">
              <label className={fieldLabelClassName} htmlFor="client-profile-limitations">
                Обмеження / застереження
              </label>
              <textarea
                id="client-profile-limitations"
                value={values.limitations}
                onChange={(event) => updateField("limitations", event.currentTarget.value)}
                disabled={isSubmitting}
                rows={4}
                maxLength={1000}
                className={`${fieldInputClassName} resize-y`}
              />
              {fieldErrors.limitations ? (
                <p className={fieldErrorClassName}>{fieldErrors.limitations}</p>
              ) : null}
            </div>

            <div className="space-y-2 md:col-span-2">
              <label className={fieldLabelClassName} htmlFor="client-profile-preferences">
                Побажання до тренувань
              </label>
              <textarea
                id="client-profile-preferences"
                value={values.trainingPreferences}
                onChange={(event) =>
                  updateField("trainingPreferences", event.currentTarget.value)
                }
                disabled={isSubmitting}
                rows={4}
                maxLength={1000}
                className={`${fieldInputClassName} resize-y`}
              />
              {fieldErrors.trainingPreferences ? (
                <p className={fieldErrorClassName}>{fieldErrors.trainingPreferences}</p>
              ) : null}
            </div>

            <div className="space-y-2 md:col-span-2">
              <label className={fieldLabelClassName} htmlFor="client-profile-additional-info">
                Додаткова інформація
              </label>
              <textarea
                id="client-profile-additional-info"
                value={values.additionalInfo}
                onChange={(event) => updateField("additionalInfo", event.currentTarget.value)}
                disabled={isSubmitting}
                rows={4}
                maxLength={1000}
                className={`${fieldInputClassName} resize-y`}
              />
              {fieldErrors.additionalInfo ? (
                <p className={fieldErrorClassName}>{fieldErrors.additionalInfo}</p>
              ) : null}
            </div>
          </div>

          <FormAlert message={loadError ?? submitError} />

          {submitSuccess ? (
            <p
              role="status"
              className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800"
            >
              {submitSuccess}
            </p>
          ) : null}

          <button
            type="submit"
            disabled={isSubmitting}
            className="inline-flex min-h-10 items-center justify-center rounded-lg bg-accent px-5 py-2 text-sm font-semibold text-white transition hover:bg-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
          >
            {isSubmitting ? "Зберігаємо..." : "Зберегти"}
          </button>
        </form>
      ) : null}

      {activeTab === "metrics" ? <BodyMetricsTab /> : null}

      {activeTab === "photos" ? <div /> : null}
    </section>
  );
}
