import type {
  ClientAssignedTrainingProgramDetails,
  ClientAssignedTrainingProgramWorkout,
} from "@/entities/training-program/model/types";
import type { WorkoutExerciseDetails } from "@/entities/workout/model/types";

export function findAssignedWorkout(
  program: ClientAssignedTrainingProgramDetails,
  workoutId: string,
): ClientAssignedTrainingProgramWorkout | null {
  return program.workouts.find((workout) => workout.workoutId === workoutId) ?? null;
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
