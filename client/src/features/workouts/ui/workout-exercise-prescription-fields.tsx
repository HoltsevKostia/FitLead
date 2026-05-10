import { fieldInputClassName, fieldLabelClassName } from "@/shared/forms/field-styles";

interface WorkoutExercisePrescriptionFieldsProps {
  idPrefix: string;
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
  idPrefix,
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
          <label className={fieldLabelClassName} htmlFor={`${idPrefix}-sets`}>
            Підходи
          </label>
          <input
            id={`${idPrefix}-sets`}
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
          <label className={fieldLabelClassName} htmlFor={`${idPrefix}-repetitions`}>
            Повторення
          </label>
          <input
            id={`${idPrefix}-repetitions`}
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
          <label className={fieldLabelClassName} htmlFor={`${idPrefix}-load`}>
            Вага, кг
          </label>
          <input
            id={`${idPrefix}-load`}
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
          <label className={fieldLabelClassName} htmlFor={`${idPrefix}-rest`}>
            Відпочинок, сек
          </label>
          <input
            id={`${idPrefix}-rest`}
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
        <label className={fieldLabelClassName} htmlFor={`${idPrefix}-note`}>
          Нотатка тренера
        </label>
        <textarea
          id={`${idPrefix}-note`}
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
