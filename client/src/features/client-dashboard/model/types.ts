export interface ClientDashboardTrainer {
  trainerId: string;
  fullName: string;
}

export interface ClientDashboardNextWorkout {
  programWorkoutId: string;
  workoutId: string;
  workoutName: string;
  weekNumber: number;
  dayNumber: number;
  orderInDay: number;
}

export interface ClientDashboardProgram {
  assignmentId: string;
  programId: string;
  title: string;
  weeksCount: number;
  daysPerWeek: number;
  assignedAtUtc: string;
  expiresAtUtc: string | null;
  completedCount: number;
  skippedCount: number;
  pendingCount: number;
  nextWorkout: ClientDashboardNextWorkout | null;
}

export interface ClientDashboard {
  trainer: ClientDashboardTrainer | null;
  activePrograms: ClientDashboardProgram[];
}
