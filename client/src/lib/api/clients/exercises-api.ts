import type {
  CreateExerciseRequest,
  Exercise,
  ExerciseListSource,
  UpdateExerciseRequest,
} from "@/entities/exercise/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const exercisesApi = {
  getExercises(source: ExerciseListSource = "all"): Promise<Exercise[]> {
    return apiRequest<Exercise[]>(`/exercises?source=${source}`);
  },

  createExercise(request: CreateExerciseRequest): Promise<string> {
    return apiRequest<string>("/exercises", {
      method: "POST",
      body: request,
    });
  },

  updateExercise(exerciseId: string, request: UpdateExerciseRequest): Promise<void> {
    return apiRequest<void>(`/exercises/${encodeURIComponent(exerciseId)}`, {
      method: "PUT",
      body: request,
      responseType: "void",
    });
  },

  deleteExercise(exerciseId: string): Promise<void> {
    return apiRequest<void>(`/exercises/${encodeURIComponent(exerciseId)}`, {
      method: "DELETE",
      responseType: "void",
    });
  },

  confirmDeleteExercise(exerciseId: string, confirmationToken: string): Promise<void> {
    return apiRequest<void>(
      `/exercises/${encodeURIComponent(exerciseId)}/deletion-confirmations`,
      {
        method: "POST",
        body: { token: confirmationToken },
        responseType: "void",
      },
    );
  },

  copyToMyLibrary(exerciseId: string): Promise<string> {
    return apiRequest<string>(
      `/exercises/${encodeURIComponent(exerciseId)}/copy-to-my-library`,
      {
        method: "POST",
      },
    );
  },
};
