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
