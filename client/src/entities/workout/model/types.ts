export interface Workout {
  id: string;
  name: string;
  trainerId: string;
}

export interface CreateWorkoutRequest {
  name: string;
}
