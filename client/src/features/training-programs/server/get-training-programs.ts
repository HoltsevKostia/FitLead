import type { TrainingProgram } from "@/entities/training-program/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainingPrograms(): Promise<TrainingProgram[]> {
  return serverApiRequest<TrainingProgram[]>("/api/training-programs");
}
