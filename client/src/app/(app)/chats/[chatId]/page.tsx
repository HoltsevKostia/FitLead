import Link from "next/link";

export default function ChatDetailsPage() {
  return (
    <section className="space-y-6">
      <Link href="/chats" className="text-sm font-medium text-accent hover:text-accent-strong">
        Назад до чатів
      </Link>

      <div className="space-y-3">
        <h1 className="text-3xl font-semibold tracking-tight">Чат</h1>
      </div>
    </section>
  );
}
