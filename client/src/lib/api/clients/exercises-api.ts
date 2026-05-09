import type {
  Exercise,
  ExerciseListSource,
  UpdateExerciseRequest,
} from "@/entities/exercise/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const exercisesApi = {
  getExercises(source: ExerciseListSource = "all"): Promise<Exercise[]> {
    return apiRequest<Exercise[]>(`/api/exercises?source=${source}`);
  },

  updateExercise(exerciseId: string, request: UpdateExerciseRequest): Promise<void> {
    return apiRequest<void>(`/api/exercises/${encodeURIComponent(exerciseId)}`, {
      method: "PUT",
      body: request,
      responseType: "void",
    });
  },

  deleteExercise(exerciseId: string): Promise<void> {
    return apiRequest<void>(`/api/exercises/${encodeURIComponent(exerciseId)}`, {
      method: "DELETE",
      responseType: "void",
    });
  },

  copyToMyLibrary(exerciseId: string): Promise<string> {
    return apiRequest<string>(
      `/api/exercises/${encodeURIComponent(exerciseId)}/copy-to-my-library`,
      {
        method: "POST",
      },
    );
  },
};
