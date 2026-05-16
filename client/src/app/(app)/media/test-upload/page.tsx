import { MediaUploadSandbox } from "@/features/media-assets/ui/media-upload-sandbox";
import { getMyMediaAssets } from "@/features/media-assets/server/get-my-media-assets";

export default async function MediaTestUploadPage() {
  const mediaAssets = await getMyMediaAssets();

  return <MediaUploadSandbox initialMediaAssets={mediaAssets} />;
}
