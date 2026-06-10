export interface VideoReportMedia {
  id: string;
  deliveryUrl: string;
  fileName: string | null;
  contentType: string;
  sizeBytes: number;
  kind: "Image" | "Video";
  durationSeconds: number | null;
  orderInReport: number;
}

export interface VideoReportDetails {
  id: string;
  chatId: string;
  clientId: string;
  trainerId: string;
  title: string;
  description: string | null;
  status: string;
  createdAtUtc: string;
  reviewedAtUtc: string | null;
  trainerFeedbackText: string | null;
  media: VideoReportMedia[];
}

export interface PendingTrainerVideoReport {
  reportId: string;
  chatId: string;
  clientId: string;
  clientName: string;
  title: string;
  description: string | null;
  mediaCount: number;
  createdAtUtc: string;
}
