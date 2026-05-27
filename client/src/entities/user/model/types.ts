import type { BodyMetricEntry } from "@/entities/body-metric/model/types";
import type { ProgressPhoto } from "@/entities/progress-photo/model/types";

export interface TrainerClient {
  clientId: string;
  email: string;
  fullName: string;
}

export interface TrainerClientProgramAccess {
  assignmentId: string;
  programId: string;
  programTitle: string;
  assignedAtUtc: string;
  expiresAtUtc: string | null;
}

export interface TrainerClientOverview {
  clientId: string;
  email: string;
  fullName: string;
  activePrograms: TrainerClientProgramAccess[];
}

export interface TrainerClientWorkspace {
  clientId: string;
  email: string;
  fullName: string;
}

export interface TrainerClientActiveProgramSummary {
  assignmentId: string;
  programId: string;
  programTitle: string;
  assignedAtUtc: string;
  expiresAtUtc: string | null;
  totalWorkouts: number;
}

export interface TrainerClientWorkoutLogCounts {
  completed: number;
  skipped: number;
  pending: number;
}

export interface TrainerClientLastWorkoutLog {
  id: string;
  assignmentId: string;
  programWorkoutId: string;
  programTitle: string;
  workoutName: string;
  weekNumber: number;
  dayNumber: number;
  orderInDay: number;
  status: "Completed" | "Skipped" | string;
  performedAtUtc: string | null;
  clientNote: string | null;
  difficultyRating: number | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface TrainerClientLastVideoReport {
  reportId: string;
  chatId: string;
  title: string;
  description: string | null;
  status: "Submitted" | "Reviewed" | string;
  mediaCount: number;
  createdAtUtc: string;
  reviewedAtUtc: string | null;
}

export interface TrainerClientVideoReport {
  reportId: string;
  chatId: string;
  title: string;
  description: string | null;
  status: "Submitted" | "Reviewed" | string;
  mediaCount: number;
  createdAtUtc: string;
  reviewedAtUtc: string | null;
}

export interface TrainerClientOverviewSummary {
  activeProgram: TrainerClientActiveProgramSummary | null;
  workoutLogCounts: TrainerClientWorkoutLogCounts;
  lastWorkoutLog: TrainerClientLastWorkoutLog | null;
  lastVideoReport: TrainerClientLastVideoReport | null;
  lastBodyMetric: BodyMetricEntry | null;
  lastProgressPhoto: ProgressPhoto | null;
}

export interface TrainerClientProgram {
  assignmentId: string;
  programId: string;
  programTitle: string;
  status: "Active" | "Revoked" | "Expired" | string;
  assignedAtUtc: string;
  expiresAtUtc: string | null;
  revokedAtUtc: string | null;
  totalWorkouts: number;
  workoutLogCounts: TrainerClientWorkoutLogCounts;
}

export interface TrainerClientWorkoutLog {
  logId: string;
  assignmentId: string;
  programId: string;
  programTitle: string;
  programWorkoutId: string;
  workoutId: string;
  workoutName: string;
  weekNumber: number;
  dayNumber: number;
  orderInDay: number;
  status: "Completed" | "Skipped" | string;
  performedAtUtc: string | null;
  difficultyRating: number | null;
  clientNote: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface TrainerClientProgress {
  bodyMetrics: BodyMetricEntry[];
  progressPhotos: ProgressPhoto[];
}

export interface ClientTrainer {
  trainerId: string;
  fullName: string;
}
