import { Equipment, MuscleGroup } from "@/entities/exercise/model/types";

export const muscleGroupOptions = Object.values(MuscleGroup).filter(
  (value): value is MuscleGroup => typeof value === "number",
);

export const equipmentOptions = Object.values(Equipment).filter(
  (value): value is Equipment => typeof value === "number",
);

export function parseOptionalNumber<TValue extends number>(value: string): TValue | null {
  if (!value) {
    return null;
  }

  const parsed = Number(value);
  return Number.isFinite(parsed) ? (parsed as TValue) : null;
}

export function formatOptionalNumber(value: number | null): string {
  return value?.toString() ?? "";
}
