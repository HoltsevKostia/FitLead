"use client";

import type { TrainerClientProfile } from "@/entities/user/model/types";
import { PlainText } from "@/shared/ui/plain-text";

interface TrainerClientProfileTabProps {
  profile: TrainerClientProfile | null;
}

const experienceLevelText: Record<string, string> = {
  Beginner: "Початковий",
  Intermediate: "Середній",
  Advanced: "Просунутий",
};

function getExperienceLevelText(value: string | null): string {
  if (!value) {
    return "Не вказано";
  }

  return experienceLevelText[value] ?? value;
}

function ProfileField({
  label,
  value,
}: {
  label: string;
  value: string | number | null;
}) {
  return (
    <div className="rounded-2xl border border-border bg-white px-5 py-5">
      <p className="text-sm font-medium text-muted">{label}</p>
      {value ? (
        typeof value === "string" ? (
          <PlainText className="mt-2 text-sm leading-6 text-foreground">{value}</PlainText>
        ) : (
          <p className="mt-2 text-sm text-foreground">{value}</p>
        )
      ) : (
        <p className="mt-2 text-sm text-muted">Не вказано</p>
      )}
    </div>
  );
}

export function TrainerClientProfileTab({ profile }: TrainerClientProfileTabProps) {
  if (!profile) {
    return null;
  }

  return (
    <div className="grid gap-4 lg:grid-cols-2">
      <ProfileField label="Ціль" value={profile.goal} />
      <ProfileField
        label="Рівень підготовки"
        value={getExperienceLevelText(profile.experienceLevel)}
      />
      <ProfileField
        label="Зріст"
        value={profile.heightCm ? `${profile.heightCm} см` : null}
      />
      <ProfileField label="Обмеження" value={profile.limitations} />
      <ProfileField label="Побажання до тренувань" value={profile.trainingPreferences} />
      <ProfileField label="Додаткова інформація" value={profile.additionalInfo} />
    </div>
  );
}
