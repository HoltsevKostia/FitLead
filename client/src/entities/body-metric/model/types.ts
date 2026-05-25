export interface BodyMetricEntry {
  id: string;
  clientId: string;
  recordedAt: string;
  weightKg: number | null;
  bodyFatPercent: number | null;
  chestCm: number | null;
  waistCm: number | null;
  hipsCm: number | null;
  armCm: number | null;
  thighCm: number | null;
  note: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface BodyMetricEntryRequest {
  recordedAt: string;
  weightKg: number | null;
  bodyFatPercent: number | null;
  chestCm: number | null;
  waistCm: number | null;
  hipsCm: number | null;
  armCm: number | null;
  thighCm: number | null;
  note: string | null;
}
