export default function AboutPage() {
  return (
    <div className="container py-12 md:py-16">
      <div className="card max-w-4xl p-8 md:p-12">
        <p className="text-sm uppercase tracking-[0.2em] text-muted">
          Про FitLead
        </p>
        <h1 className="mt-4 text-4xl font-semibold tracking-tight md:text-5xl">
          Платформа для практичної взаємодії між тренером і клієнтом.
        </h1>
        <div className="mt-8 space-y-5 text-lg leading-8 text-muted">
          <p>
            FitLead побудований навколо основних робочих сценаріїв тренера:
            створення вправ, складання тренувань, публікація програм і
            керування запрошеннями для клієнтів.
          </p>
        </div>
      </div>
    </div>
  );
}
