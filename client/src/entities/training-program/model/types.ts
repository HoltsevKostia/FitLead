export interface TrainingProgram {
  id: string;
  title: string;
  weeksCount: number;
  daysPerWeek: number;
}

export interface CreateTrainingProgramRequest {
  title: string;
  weeksCount: number;
  daysPerWeek: number;
}

export interface AddTrainingProgramWorkoutRequest {
  workoutId: string;
  weekNumber: number;
  dayNumber: number;
}

export interface TrainingProgramWorkout {
  id: string;
  workoutId: string;
  workoutName: string;
  trainerId: string;
  weekNumber: number;
  dayNumber: number;
  orderInDay: number;
}
