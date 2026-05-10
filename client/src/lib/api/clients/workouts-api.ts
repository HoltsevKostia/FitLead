import type {
  AddWorkoutExerciseRequest,
  CreateWorkoutRequest,
  UpdateWorkoutExerciseRequest,
  Workout,
} from "@/entities/workout/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const workoutsApi = {
  getWorkouts(): Promise<Workout[]> {
    return apiRequest<Workout[]>("/api/workouts");
  },

  createWorkout(request: CreateWorkoutRequest): Promise<string> {
    return apiRequest<string>("/api/workouts", {
      method: "POST",
      body: request,
    });
  },

  addExercise(workoutId: string, request: AddWorkoutExerciseRequest): Promise<string> {
    return apiRequest<string>(`/api/workouts/${encodeURIComponent(workoutId)}/exercises`, {
      method: "POST",
      body: request,
    });
  },

  updateExercise(
    workoutId: string,
    workoutExerciseId: string,
    request: UpdateWorkoutExerciseRequest,
  ): Promise<void> {
    return apiRequest<void>(
      `/api/workouts/${encodeURIComponent(workoutId)}/exercises/${encodeURIComponent(workoutExerciseId)}`,
      {
        method: "PUT",
        body: request,
        responseType: "void",
      },
    );
  },

  removeExercise(workoutId: string, workoutExerciseId: string): Promise<void> {
    return apiRequest<void>(
      `/api/workouts/${encodeURIComponent(workoutId)}/exercises/${encodeURIComponent(workoutExerciseId)}`,
      {
        method: "DELETE",
        responseType: "void",
      },
    );
  },
};
