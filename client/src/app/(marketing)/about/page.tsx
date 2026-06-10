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
            FitLead об’єднує інструменти для ведення клієнтів, створення
            тренувальних програм, відстеження прогресу та дистанційного
            супроводу через чат і відеозвіти.
          </p>
        </div>

        <div className="mt-10 grid gap-4 md:grid-cols-3">
          <section className="rounded-xl border border-border bg-surface p-5">
            <h2 className="text-xl font-semibold text-foreground">Для тренера</h2>
            <p className="mt-3 leading-7 text-muted">
              FitLead допомагає організувати роботу з клієнтами в одному
              просторі: створювати вправи й тренування, складати програми та
              стежити за їх виконанням.
            </p>
          </section>

          <section className="rounded-xl border border-border bg-surface p-5">
            <h2 className="text-xl font-semibold text-foreground">Для клієнта</h2>
            <p className="mt-3 leading-7 text-muted">
              Клієнт бачить призначені програми й наступні тренування, відмічає
              виконання, веде власні метрики та додає фотографії прогресу.
            </p>
          </section>

          <section className="rounded-xl border border-border bg-surface p-5">
            <h2 className="text-xl font-semibold text-foreground">
              Постійний супровід
            </h2>
            <p className="mt-3 leading-7 text-muted">
              Чат і відеозвіти підтримують зв’язок поза тренуваннями. Клієнт
              може надіслати відео виконання, а тренер — переглянути його та
              залишити відгук.
            </p>
          </section>
        </div>
      </div>
    </div>
  );
}
