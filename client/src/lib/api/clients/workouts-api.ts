import type { CreateWorkoutRequest, Workout } from "@/entities/workout/model/types";
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
};
