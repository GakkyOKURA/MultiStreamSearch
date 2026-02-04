import type { Platform } from "./platform";

export type VideoDataResponse = {
    items: VideoDataDTO[];
};

export type VideoDataDTO = {
    videoId: string;
    videoTitle: string;
    channelId: string;
    channelName: string
    searchDescription: string;
    channelDescription: string
    searchHighTumbnail: VideoHighThumbnailDTO;
    channelHighThumbnail: VideoHighThumbnailDTO;
    keywords: string;
    bannerExternalUrl: string;
    topicCategories: string[];
    platform: Platform;
};

export type VideoHighThumbnailDTO = {
    url: string;
    width: string;
    height: string;
}

