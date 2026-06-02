import { SkeletonBlock, SkeletonCard, SkeletonText } from "@/shared/ui/skeleton";

function HeaderSkeleton() {
  return (
    <div className="space-y-4 rounded-2xl border border-border bg-white px-5 py-5">
      <SkeletonText className="h-4 w-32" />

      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div className="min-w-0 space-y-3">
          <SkeletonText className="h-9 w-64 max-w-full" />
          <SkeletonText className="h-4 w-56 max-w-full" />
        </div>

        <div className="flex flex-col gap-3 sm:flex-row sm:items-start">
          <SkeletonBlock className="h-10 w-32 rounded-full" />
          <SkeletonBlock className="h-10 w-44 rounded-full" />
        </div>
      </div>
    </div>
  );
}

function TabsSkeleton() {
  return (
    <div className="flex gap-2 overflow-hidden border-b border-border pb-2">
      {Array.from({ length: 6 }).map((_, index) => (
        <SkeletonBlock
          key={index}
          className="h-10 w-28 shrink-0 rounded-full"
        />
      ))}
    </div>
  );
}

function OverviewCardSkeleton({ wide = false }: { wide?: boolean }) {
  return (
    <SkeletonCard className={wide ? "xl:col-span-2" : undefined}>
      <div className="space-y-4">
        <div className="flex items-start justify-between gap-4">
          <SkeletonText className="h-5 w-40" />
          <SkeletonText className="h-4 w-20" />
        </div>
        <div className="space-y-3">
          <SkeletonText className="h-6 w-3/4" />
          <SkeletonText className="w-full" />
          <SkeletonText className="w-2/3" />
        </div>
      </div>
    </SkeletonCard>
  );
}

export default function TrainerClientWorkspaceLoading() {
  return (
    <section className="space-y-6">
      <HeaderSkeleton />
      <TabsSkeleton />

      <div className="grid gap-4 xl:grid-cols-2">
        <OverviewCardSkeleton />
        <OverviewCardSkeleton />
        <OverviewCardSkeleton />
        <OverviewCardSkeleton />
        <OverviewCardSkeleton wide />
      </div>
    </section>
  );
}
