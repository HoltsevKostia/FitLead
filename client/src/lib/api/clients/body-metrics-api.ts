import type {
  BodyMetricEntry,
  BodyMetricEntryRequest,
} from "@/entities/body-metric/model/types";
import { apiRequest } from "@/lib/api/http-client";

export const bodyMetricsApi = {
  list(): Promise<BodyMetricEntry[]> {
    return apiRequest<BodyMetricEntry[]>("/client/body-metrics");
  },

  create(request: BodyMetricEntryRequest): Promise<BodyMetricEntry> {
    return apiRequest<BodyMetricEntry>("/client/body-metrics", {
      method: "POST",
      body: request,
    });
  },

  update(entryId: string, request: BodyMetricEntryRequest): Promise<BodyMetricEntry> {
    return apiRequest<BodyMetricEntry>(`/client/body-metrics/${entryId}`, {
      method: "PUT",
      body: request,
    });
  },

  delete(entryId: string): Promise<void> {
    return apiRequest<void>(`/client/body-metrics/${entryId}`, {
      method: "DELETE",
      responseType: "void",
    });
  },
};
