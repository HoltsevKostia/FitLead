import type {
  CreateTrainingProgramRequest,
  TrainingProgram,
} from "@/entities/training-program/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const trainingProgramsApi = {
  getTrainingPrograms(): Promise<TrainingProgram[]> {
    return apiRequest<TrainingProgram[]>("/api/training-programs");
  },

  createTrainingProgram(request: CreateTrainingProgramRequest): Promise<string> {
    return apiRequest<string>("/api/training-programs", {
      method: "POST",
      body: request,
    });
  },
};
