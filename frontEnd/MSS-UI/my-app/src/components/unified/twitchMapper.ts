import type { TwitchClipSearchDto, TwitchStreamSearchDto } from "../twitch/TwitchDto";
import type { UnifiedVideo } from "./unifiedVideo";

export function mapTwitchStreamToUnified(item: TwitchStreamSearchDto): UnifiedVideo {
  return {
    id: item.userLogin,
    title: item.title,
    thumbnailUrl: item.thumbnailUrl.replace("{width}", "320").replace("{height}", "180"),
    url: `https://www.twitch.tv/${item.userLogin}`,
    source: "twitch",
    type: "twitchStream",
    channelName: item.userName,
  };
}

export function mapTwitchClipToUnified(item: TwitchClipSearchDto): UnifiedVideo {
  return {
    id: item.id,
    title: item.title,
    thumbnailUrl: item.thumbnailUrl,
    url: item.url,
    source: "twitch",
    type: "twitchClip",
    channelName: item.broadcasterName,
  };
}
