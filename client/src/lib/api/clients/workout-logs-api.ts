import type { LogWorkoutRequest, WorkoutLogPreview } from "@/entities/training-program/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const workoutLogsApi = {
  logWorkout(
    assignmentId: string,
    programWorkoutId: string,
    request: LogWorkoutRequest,
  ): Promise<WorkoutLogPreview> {
    return apiRequest<WorkoutLogPreview>(
      `/client/training-program-assignments/${encodeURIComponent(assignmentId)}/workouts/${encodeURIComponent(programWorkoutId)}/log`,
      {
        method: "PUT",
        body: request,
      },
    );
  },
};
