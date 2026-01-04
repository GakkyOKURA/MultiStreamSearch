import type { UnifiedVideo } from "./unifiedVideo";

export function mapYouTubeToUnified(item: any): UnifiedVideo {
  return {
    id: item.id.videoId,
    title: item.snippet.title,
    thumbnailUrl: item.snippet.thumbnails.medium.url,
    url: `https://www.youtube.com/watch?v=${item.id.videoId}`,
    source: "youtube",
    type: "youtubeLive",
    channelName: item.snippet.channelTitle,
    publishedAt: item.snippet.publishedAt
  };
}

export function mapShortToUnified(item: any): UnifiedVideo {
  return {
    id: item.id.videoId,
    title: item.snippet.title,
    thumbnailUrl: item.snippet.thumbnails.medium.url,
    url: `https://www.youtube.com/watch?v=${item.id.videoId}`,
    source: "youtube",
    type: "short",
    channelName: item.snippet.channelTitle,
    publishedAt: item.snippet.publishedAt
  };
}