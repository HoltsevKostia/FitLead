import type { TrainingProgram } from "@/entities/training-program/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainingProgram(programId: string): Promise<TrainingProgram> {
  return serverApiRequest<TrainingProgram>(`/api/training-programs/${programId}`);
}
