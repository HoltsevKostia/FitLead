import { fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";

interface WorkoutExercisePrescriptionFieldsProps {
  sets: string;
  repetitions: string;
  loadKg: string;
  restSeconds: string;
  trainerNote: string;
  isSubmitting: boolean;
  onSetsChange: (value: string) => void;
  onRepetitionsChange: (value: string) => void;
  onLoadKgChange: (value: string) => void;
  onRestSecondsChange: (value: string) => void;
  onTrainerNoteChange: (value: string) => void;
}

export function WorkoutExercisePrescriptionFields({
  sets,
  repetitions,
  loadKg,
  restSeconds,
  trainerNote,
  isSubmitting,
  onSetsChange,
  onRepetitionsChange,
  onLoadKgChange,
  onRestSecondsChange,
  onTrainerNoteChange,
}: WorkoutExercisePrescriptionFieldsProps) {
  return (
    <>
      <div className="grid gap-4 md:grid-cols-4">
        <div className="space-y-2">
          <label className={fieldLabelClassName} htmlFor="workout-exercise-sets">
            Підходи
          </label>
          <input
            id="workout-exercise-sets"
            type="number"
            min={1}
            step={1}
            required
            value={sets}
            onChange={(event) => onSetsChange(event.target.value)}
            disabled={isSubmitting}
            className={fieldInputClassName}
          />
        </div>

        <div className="space-y-2">
          <label className={fieldLabelClassName} htmlFor="workout-exercise-repetitions">
            Повторення
          </label>
          <input
            id="workout-exercise-repetitions"
            type="number"
            min={1}
            step={1}
            required
            value={repetitions}
            onChange={(event) => onRepetitionsChange(event.target.value)}
            disabled={isSubmitting}
            className={fieldInputClassName}
          />
        </div>

        <div className="space-y-2">
          <label className={fieldLabelClassName} htmlFor="workout-exercise-load">
            Вага, кг
          </label>
          <input
            id="workout-exercise-load"
            type="number"
            min={0}
            step="0.5"
            value={loadKg}
            onChange={(event) => onLoadKgChange(event.target.value)}
            disabled={isSubmitting}
            className={fieldInputClassName}
          />
        </div>

        <div className="space-y-2">
          <label className={fieldLabelClassName} htmlFor="workout-exercise-rest">
            Відпочинок, сек
          </label>
          <input
            id="workout-exercise-rest"
            type="number"
            min={0}
            step={1}
            required
            value={restSeconds}
            onChange={(event) => onRestSecondsChange(event.target.value)}
            disabled={isSubmitting}
            className={fieldInputClassName}
          />
        </div>
      </div>

      <div className="space-y-2">
        <label className={fieldLabelClassName} htmlFor="workout-exercise-note">
          Нотатка тренера
        </label>
        <textarea
          id="workout-exercise-note"
          value={trainerNote}
          onChange={(event) => onTrainerNoteChange(event.target.value)}
          disabled={isSubmitting}
          maxLength={1000}
          rows={3}
          className={`${fieldInputClassName} resize-y`}
        />
      </div>
    </>
  );
}
