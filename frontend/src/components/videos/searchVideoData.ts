import type { VideoDataResponse } from "./videoData";
import type { VideoDataDTO } from "./videoData";
import type { VideoWithSummaryDTO, VideoWithSummaryResponse } from "./videoWithSummary";

export const SearchVideoData = async (): Promise<VideoDataDTO[]> => {
    const baseUrl = import.meta.env.VITE_API_BASE_URL ?? "";
    const url = `${baseUrl}/api/videos`;

    const result = await fetch(url);
    if(!result.ok){
        throw new Error(`API error ${result.status}`);
    }

    const data: VideoDataResponse = await result.json();
    return data.items;
};

export const SearchVideoWithAnalysis = async (): Promise<VideoWithSummaryDTO[]> => {
    const baseUrl = import.meta.env.VITE_API_BASE_URL ?? "";
    const url = `${baseUrl}/api/videos/ai`;

    const result = await fetch(url);
    if(!result.ok){
        throw new Error(`API error ${result.status}`);
    }

    const data: VideoWithSummaryResponse = await result.json();
    return data.items;
}