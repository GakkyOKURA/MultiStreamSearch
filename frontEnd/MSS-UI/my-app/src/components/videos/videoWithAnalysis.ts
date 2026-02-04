import type { Platform } from "./platform";

export type VideoWithAnalysisResponse = {
    items: VideoWithAnalysisDTO[];
}

export type VideoWithAnalysisDTO = {
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
    aiDescription: string;
};

export type VideoHighThumbnailDTO = {
    url: string;
    width: string;
    height: string;
}