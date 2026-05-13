import type { ClientAssignedTrainingProgram } from "@/entities/training-program/model/types";
import { serverApiRequest } from "@/lib/api/server-api";

export async function getClientAssignedTrainingPrograms(): Promise<
  ClientAssignedTrainingProgram[]
> {
  return serverApiRequest<ClientAssignedTrainingProgram[]>("/api/client/training-programs");
}
