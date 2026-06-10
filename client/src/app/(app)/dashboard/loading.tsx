import { SkeletonCard, SkeletonText } from "@/shared/ui/skeleton";

function SummaryCardSkeleton() {
  return (
    <SkeletonCard className="min-h-36">
      <SkeletonText className="w-28" />
      <SkeletonText className="mt-4 h-8 w-12" />
      <div className="mt-4 space-y-2">
        <SkeletonText className="w-full" />
        <SkeletonText className="w-2/3" />
      </div>
    </SkeletonCard>
  );
}

export default function DashboardLoading() {
  return (
    <section className="space-y-6">
      <div className="space-y-3">
        <SkeletonText className="w-20" />
        <SkeletonText className="h-10 w-48 max-w-full" />
      </div>

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        {Array.from({ length: 4 }).map((_, index) => (
          <SummaryCardSkeleton key={index} />
        ))}
      </div>
    </section>
  );
}
