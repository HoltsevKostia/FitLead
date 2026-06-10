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

      <div className="space-y-3">
        <SkeletonText className="h-6 w-32" />
        <div className="grid gap-3 lg:grid-cols-2">
          {Array.from({ length: 2 }).map((_, index) => (
            <SkeletonCard key={index} className="min-h-48">
              <SkeletonText className="w-40 max-w-full" />
              <SkeletonText className="mt-3 w-56 max-w-full" />
              <SkeletonText className="mt-8 h-10 w-40 max-w-full" />
            </SkeletonCard>
          ))}
        </div>
      </div>
    </section>
  );
}
