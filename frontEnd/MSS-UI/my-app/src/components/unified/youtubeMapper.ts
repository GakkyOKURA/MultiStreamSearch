import type { YouTubeSearchItemDto } from "../youtube/YouTubeDto";
import type { UnifiedVideo } from "./unifiedVideo";

export function mapYouTubeToUnified(item: YouTubeSearchItemDto): UnifiedVideo {
  return {
    id: item.id.videoId,
    title: item.snippet.title,
    thumbnailUrl: item.snippet.thumbnails.medium.url,
    url: `https://www.youtube.com/watch?v=${item.id.videoId}`,
    source: "youtube",
    type: "youtubeLiveStream",
    channelName: item.snippet.channelTitle,
  };
}

export function mapShortToUnified(item: YouTubeSearchItemDto): UnifiedVideo {
  return {
    id: item.id.videoId,
    title: item.snippet.title,
    thumbnailUrl: item.snippet.thumbnails.medium.url,
    url: `https://www.youtube.com/watch?v=${item.id.videoId}`,
    source: "youtube",
    type: "youtubeShort",
    channelName: item.snippet.channelTitle,
  };
}