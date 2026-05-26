import type {
  BodyMetricEntry,
  BodyMetricEntryRequest,
} from "@/entities/body-metric/model/types";

export interface BodyMetricFormValues {
  recordedAt: string;
  weightKg: string;
  bodyFatPercent: string;
  chestCm: string;
  waistCm: string;
  hipsCm: string;
  armCm: string;
  thighCm: string;
  note: string;
}

export type BodyMetricFieldErrors = Partial<Record<keyof BodyMetricFormValues, string>>;

export const metricFields: Array<{
  name: keyof Omit<BodyMetricFormValues, "recordedAt" | "note">;
  label: string;
  suffix: string;
  max: number;
  step: string;
}> = [
  { name: "weightKg", label: "Вага", suffix: "кг", max: 500, step: "0.1" },
  { name: "bodyFatPercent", label: "Жир", suffix: "%", max: 80, step: "0.1" },
  { name: "chestCm", label: "Груди", suffix: "см", max: 300, step: "0.1" },
  { name: "waistCm", label: "Талія", suffix: "см", max: 300, step: "0.1" },
  { name: "hipsCm", label: "Стегна", suffix: "см", max: 300, step: "0.1" },
  { name: "armCm", label: "Рука", suffix: "см", max: 300, step: "0.1" },
  { name: "thighCm", label: "Стегно", suffix: "см", max: 300, step: "0.1" },
];

export function getTodayDateInputValue(): string {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, "0");
  const day = String(now.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

export function normalizeDateInputValue(value: string): string {
  const dateOnly = value.includes("T") ? value.split("T")[0] : value;

  return /^\d{4}-\d{2}-\d{2}$/.test(dateOnly) ? dateOnly : "";
}

export function createEmptyValues(): BodyMetricFormValues {
  return {
    recordedAt: getTodayDateInputValue(),
    weightKg: "",
    bodyFatPercent: "",
    chestCm: "",
    waistCm: "",
    hipsCm: "",
    armCm: "",
    thighCm: "",
    note: "",
  };
}

export function createPrefilledValues(entries: BodyMetricEntry[]): BodyMetricFormValues {
  const values = createEmptyValues();

  for (const field of metricFields) {
    const latestEntryWithValue = entries.find((entry) => entry[field.name] !== null);
    const latestValue = latestEntryWithValue?.[field.name] ?? null;
    values[field.name] = formatDecimal(latestValue);
  }

  return values;
}

export function normalizeOptionalText(value: string): string | null {
  const trimmedValue = value.trim();
  return trimmedValue ? trimmedValue : null;
}

export function parseOptionalDecimal(value: string): number | null {
  const trimmedValue = value.trim();
  if (!trimmedValue) {
    return null;
  }

  const parsed = Number(trimmedValue.replace(",", "."));
  return Number.isFinite(parsed) ? parsed : Number.NaN;
}

export function formatDecimal(value: number | null): string {
  return value === null ? "" : String(value);
}

export function formatBodyMetricDate(value: string): string {
  const normalizedValue = normalizeDateInputValue(value);
  const [year, month, day] = normalizedValue.split("-").map(Number);
  if (!year || !month || !day) {
    return value;
  }

  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(year, month - 1, day));
}

export function getInitialValues(entry: BodyMetricEntry | null): BodyMetricFormValues {
  if (!entry) {
    return createEmptyValues();
  }

  return {
    recordedAt: normalizeDateInputValue(entry.recordedAt),
    weightKg: formatDecimal(entry.weightKg),
    bodyFatPercent: formatDecimal(entry.bodyFatPercent),
    chestCm: formatDecimal(entry.chestCm),
    waistCm: formatDecimal(entry.waistCm),
    hipsCm: formatDecimal(entry.hipsCm),
    armCm: formatDecimal(entry.armCm),
    thighCm: formatDecimal(entry.thighCm),
    note: entry.note ?? "",
  };
}

export function validateBodyMetricForm(values: BodyMetricFormValues): BodyMetricFieldErrors {
  const errors: BodyMetricFieldErrors = {};

  if (!normalizeDateInputValue(values.recordedAt)) {
    errors.recordedAt = "Оберіть дату запису.";
  }

  for (const field of metricFields) {
    const value = values[field.name];
    const parsed = parseOptionalDecimal(value);

    if (
      value.trim() &&
      (!Number.isFinite(parsed) || parsed === null || parsed <= 0 || parsed > field.max)
    ) {
      errors[field.name] = `${field.label} має бути числом від 1 до ${field.max}.`;
    }
  }

  if (values.note.trim().length > 1000) {
    errors.note = "Нотатка має бути не довшою за 1000 символів.";
  }

  const hasMetricValue = metricFields.some((field) => values[field.name].trim().length > 0);
  if (!hasMetricValue && values.note.trim().length === 0) {
    errors.note = "Заповніть хоча б одну метрику або нотатку.";
  }

  return errors;
}

export function toBodyMetricRequest(values: BodyMetricFormValues): BodyMetricEntryRequest {
  return {
    recordedAt: normalizeDateInputValue(values.recordedAt),
    weightKg: parseOptionalDecimal(values.weightKg),
    bodyFatPercent: parseOptionalDecimal(values.bodyFatPercent),
    chestCm: parseOptionalDecimal(values.chestCm),
    waistCm: parseOptionalDecimal(values.waistCm),
    hipsCm: parseOptionalDecimal(values.hipsCm),
    armCm: parseOptionalDecimal(values.armCm),
    thighCm: parseOptionalDecimal(values.thighCm),
    note: normalizeOptionalText(values.note),
  };
}

export function getMetricSummary(entry: BodyMetricEntry): string[] {
  const items = [
    entry.weightKg === null ? null : `Вага: ${entry.weightKg} кг`,
    entry.bodyFatPercent === null ? null : `Жир: ${entry.bodyFatPercent}%`,
    entry.chestCm === null ? null : `Груди: ${entry.chestCm} см`,
    entry.waistCm === null ? null : `Талія: ${entry.waistCm} см`,
    entry.hipsCm === null ? null : `Стегна: ${entry.hipsCm} см`,
    entry.armCm === null ? null : `Рука: ${entry.armCm} см`,
    entry.thighCm === null ? null : `Стегно: ${entry.thighCm} см`,
  ];

  return items.filter((item): item is string => item !== null);
}
