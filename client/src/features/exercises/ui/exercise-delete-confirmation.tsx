import type { ExerciseDeleteConflict } from "@/features/exercises/model/delete-conflict";

interface ExerciseDeleteConfirmationProps {
  conflict: ExerciseDeleteConflict;
  isConfirming: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ExerciseDeleteConfirmation({
  conflict,
  isConfirming,
  onConfirm,
  onCancel,
}: ExerciseDeleteConfirmationProps) {
  return (
    <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-4 text-sm text-amber-950 md:absolute md:right-0 md:top-full md:z-20 md:mt-2 md:w-96 md:shadow-lg">
      <p className="font-semibold">Потрібне підтвердження видалення</p>
      <p className="mt-2 leading-6">
        Ця вправа використовується {conflict.workoutExerciseCount} раз(ів) у
        тренуваннях. Якщо підтвердити видалення, вона буде прибрана з відповідних
        тренувань.
      </p>
      <div className="mt-4 flex flex-col gap-2 sm:flex-row">
        <button
          type="button"
          onClick={onConfirm}
          disabled={isConfirming}
          className="rounded-full bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-700 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isConfirming ? "Видаляємо..." : "Підтвердити видалення"}
        </button>
        <button
          type="button"
          onClick={onCancel}
          disabled={isConfirming}
          className="rounded-full border border-amber-300 px-4 py-2 text-sm font-medium transition hover:bg-amber-100 disabled:cursor-not-allowed disabled:opacity-70"
        >
          Скасувати
        </button>
      </div>
    </div>
  );
}
