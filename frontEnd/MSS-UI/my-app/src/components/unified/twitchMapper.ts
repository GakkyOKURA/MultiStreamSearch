import type { UnifiedVideo } from "./unifiedVideo";

export function mapTwitchStreamToUnified(item: any): UnifiedVideo {
  return {
    id: item.user_login,
    title: item.title,
    thumbnailUrl: item.thumbnail_url.replace("{width}", "320").replace("{height}", "180"),
    url: `https://www.twitch.tv/${item.user_login}`,
    source: "twitch",
    type: "twitchLive",
    channelName: item.user_name,
    viewerCount: item.viewer_count
  };
}

export function mapTwitchClipToUnified(item: any): UnifiedVideo {
  return {
    id: item.id,
    title: item.title,
    thumbnailUrl: item.thumbnail_url,
    url: item.url,
    source: "twitch",
    type: "clip",
    channelName: item.broadcaster_name,
    publishedAt: item.created_at
  };
}
