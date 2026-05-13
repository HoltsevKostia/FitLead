import type { ClientAssignedTrainingProgramDetails } from "@/entities/training-program/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getClientAssignedTrainingProgramDetails(
  assignmentId: string,
): Promise<ClientAssignedTrainingProgramDetails> {
  return serverApiRequest<ClientAssignedTrainingProgramDetails>(
    `/api/client/training-programs/${encodeURIComponent(assignmentId)}`,
  );
}
