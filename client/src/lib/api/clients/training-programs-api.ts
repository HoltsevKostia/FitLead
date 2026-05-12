import type {
  AddTrainingProgramWorkoutRequest,
  CreateTrainingProgramRequest,
  TrainingProgram,
  TrainingProgramWorkout,
} from "@/entities/training-program/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const trainingProgramsApi = {
  getTrainingPrograms(): Promise<TrainingProgram[]> {
    return apiRequest<TrainingProgram[]>("/api/training-programs");
  },

  getTrainingProgram(programId: string): Promise<TrainingProgram> {
    return apiRequest<TrainingProgram>(
      `/api/training-programs/${encodeURIComponent(programId)}`,
    );
  },

  createTrainingProgram(request: CreateTrainingProgramRequest): Promise<string> {
    return apiRequest<string>("/api/training-programs", {
      method: "POST",
      body: request,
    });
  },

  getProgramWorkouts(programId: string): Promise<TrainingProgramWorkout[]> {
    return apiRequest<TrainingProgramWorkout[]>(
      `/api/training-programs/${encodeURIComponent(programId)}/workouts`,
    );
  },

  addWorkout(
    programId: string,
    request: AddTrainingProgramWorkoutRequest,
  ): Promise<void> {
    return apiRequest<void>(
      `/api/training-programs/${encodeURIComponent(programId)}/workouts`,
      {
        method: "POST",
        body: request,
        responseType: "void",
      },
    );
  },
};
