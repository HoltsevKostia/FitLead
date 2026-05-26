export type ClientExperienceLevel = "Beginner" | "Intermediate" | "Advanced";

export interface ClientProfile {
  clientId: string;
  goal: string | null;
  experienceLevel: ClientExperienceLevel | null;
  heightCm: number | null;
  limitations: string | null;
  trainingPreferences: string | null;
  additionalInfo: string | null;
  createdAtUtc: string | null;
  updatedAtUtc: string | null;
}

export interface UpdateClientProfileRequest {
  goal: string | null;
  experienceLevel: ClientExperienceLevel | null;
  heightCm: number | null;
  limitations: string | null;
  trainingPreferences: string | null;
  additionalInfo: string | null;
}
