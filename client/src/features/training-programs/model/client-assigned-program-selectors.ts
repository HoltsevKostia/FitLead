import type {
  ClientAssignedTrainingProgramDetails,
  ClientAssignedTrainingProgramWorkout,
} from "@/entities/training-program/model/types";
import type { WorkoutExerciseDetails } from "@/entities/workout/model/types";

export function findAssignedWorkout(
  program: ClientAssignedTrainingProgramDetails,
  programWorkoutId: string,
): ClientAssignedTrainingProgramWorkout | null {
  return program.workouts.find((workout) => workout.id === programWorkoutId) ?? null;
}

export function findAssignedWorkoutExercise(
  workout: ClientAssignedTrainingProgramWorkout,
  workoutExerciseId: string,
): WorkoutExerciseDetails | null {
  return (
    workout.exercises.find((exercise) => exercise.workoutExerciseId === workoutExerciseId) ??
    null
  );
}
