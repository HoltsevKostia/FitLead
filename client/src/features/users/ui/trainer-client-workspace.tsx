"use client";

import Link from "next/link";

import type {
  TrainerClientProgram,
  TrainerClientProgress,
  TrainerClientWorkoutLog,
  TrainerClientOverviewSummary,
  TrainerClientWorkspace as TrainerClientWorkspaceModel,
} from "@/entities/user/model/types";
import { OpenChatButton } from "@/features/chats/ui/open-chat-button";
import { TrainerClientOverviewTab } from "@/features/users/ui/trainer-client-overview-tab";
import { TrainerClientProgramsTab } from "@/features/users/ui/trainer-client-programs-tab";
import { TrainerClientProgressTab } from "@/features/users/ui/trainer-client-progress-tab";
import { TrainerClientWorkoutLogsTab } from "@/features/users/ui/trainer-client-workout-logs-tab";

interface TrainerClientWorkspaceProps {
  client: TrainerClientWorkspaceModel;
  activeTab?: string;
  overview?: TrainerClientOverviewSummary | null;
  programs?: TrainerClientProgram[] | null;
  workoutLogs?: TrainerClientWorkoutLog[] | null;
  progress?: TrainerClientProgress | null;
}

const tabs = [
  { id: "overview", label: "Огляд" },
  { id: "programs", label: "Програми" },
  { id: "workout-logs", label: "Журнал тренувань" },
  { id: "progress", label: "Прогрес" },
  { id: "video-reports", label: "Відео-звіти" },
  { id: "profile", label: "Профіль" },
] as const;

function getActiveTab(value: string | null): string {
  return tabs.some((tab) => tab.id === value) ? value! : tabs[0].id;
}

function TabContent({
  activeTab,
  overview,
  programs,
  workoutLogs,
  progress,
}: {
  activeTab: string;
  overview: TrainerClientOverviewSummary | null;
  programs: TrainerClientProgram[] | null;
  workoutLogs: TrainerClientWorkoutLog[] | null;
  progress: TrainerClientProgress | null;
}) {
  if (activeTab === "overview") {
    return <TrainerClientOverviewTab overview={overview} />;
  }

  if (activeTab === "programs") {
    return <TrainerClientProgramsTab programs={programs} />;
  }

  if (activeTab === "workout-logs") {
    return <TrainerClientWorkoutLogsTab logs={workoutLogs} />;
  }

  if (activeTab === "progress") {
    return <TrainerClientProgressTab progress={progress} />;
  }

  return null;
}

export function TrainerClientWorkspace({
  client,
  activeTab: selectedTab,
  overview = null,
  programs = null,
  workoutLogs = null,
  progress = null,
}: TrainerClientWorkspaceProps) {
  const activeTab = getActiveTab(selectedTab ?? null);

  return (
    <section className="space-y-6">
      <div className="space-y-4 rounded-2xl border border-border bg-white px-5 py-5">
        <Link href="/clients" className="text-sm font-medium text-accent hover:text-accent-strong">
          Назад до клієнтів
        </Link>

        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div className="min-w-0 space-y-2">
            <h1 className="break-words text-3xl font-semibold tracking-tight text-foreground">
              {client.fullName}
            </h1>
            <p className="break-words text-sm text-muted">{client.email}</p>
          </div>

          <div className="flex flex-col gap-3 sm:flex-row sm:items-start">
            <OpenChatButton
              targetId={client.clientId}
              targetType="client"
              label="Відкрити чат"
            />
            <Link
              href={`/training-programs?assignClientId=${client.clientId}`}
              className="w-fit rounded-full bg-accent px-4 py-2 text-sm font-medium text-white transition hover:bg-accent-strong"
            >
              Призначити програму
            </Link>
          </div>
        </div>
      </div>

      <nav
        aria-label="Розділи клієнта"
        className="flex gap-2 overflow-x-auto border-b border-border pb-2"
      >
        {tabs.map((tab) => {
          const isActive = tab.id === activeTab;

          return (
            <Link
              key={tab.id}
              href={`/clients/${client.clientId}?tab=${tab.id}`}
              aria-current={isActive ? "page" : undefined}
              className={`shrink-0 rounded-full px-4 py-2 text-sm font-medium transition ${
                isActive
                  ? "bg-accent text-white"
                  : "border border-border bg-white text-foreground hover:bg-surface-strong"
              }`}
            >
              {tab.label}
            </Link>
          );
        })}
      </nav>

      <TabContent
        activeTab={activeTab}
        overview={overview}
        programs={programs}
        workoutLogs={workoutLogs}
        progress={progress}
      />
    </section>
  );
}
