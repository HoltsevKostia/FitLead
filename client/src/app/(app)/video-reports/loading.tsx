import { SkeletonCard, SkeletonText } from "@/shared/ui/skeleton";

function PendingReportCardSkeleton() {
  return (
    <SkeletonCard className="p-4 sm:p-5">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 flex-1 space-y-3">
          <div className="flex gap-2">
            <SkeletonText className="h-7 w-32" />
            <SkeletonText className="h-7 w-20" />
          </div>
          <SkeletonText className="h-6 w-56 max-w-full" />
          <SkeletonText className="w-36" />
          <SkeletonText className="w-72 max-w-full" />
        </div>
        <SkeletonText className="h-10 w-full sm:w-24" />
      </div>
      <div className="mt-4 space-y-2 rounded-xl border border-border p-4">
        <SkeletonText className="w-full" />
        <SkeletonText className="w-3/4" />
      </div>
    </SkeletonCard>
  );
}

export default function VideoReportsLoading() {
  return (
    <section className="space-y-6">
      <div className="space-y-2">
        <SkeletonText className="h-9 w-52 max-w-full" />
        <SkeletonText className="w-72 max-w-full" />
      </div>

      <div className="space-y-4">
        {Array.from({ length: 3 }).map((_, index) => (
          <PendingReportCardSkeleton key={index} />
        ))}
      </div>
    </section>
  );
}
