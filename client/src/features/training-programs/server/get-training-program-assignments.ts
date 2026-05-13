import type { TrainingProgramAssignment } from "@/entities/training-program/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getTrainingProgramAssignments(
  programId: string,
): Promise<TrainingProgramAssignment[]> {
  return serverApiRequest<TrainingProgramAssignment[]>(
    `/api/training-programs/${encodeURIComponent(programId)}/assignments`,
  );
}
