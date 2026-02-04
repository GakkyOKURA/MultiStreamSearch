import type { VideoDataResponse } from "./videoData";
import type { VideoDataDTO } from "./videoData";
import type { VideoWithAnalysisResponse } from "./videoWithAnalysis";
import type { VideoWithAnalysisDTO } from "./videoWithAnalysis";

export const SearchVideoData = async (): Promise<VideoDataDTO[]> => {
    const url = "https://localhost:7138/api/videos";

    const result = await fetch(url);
    if(!result.ok){
        throw new Error(`API error ${result.status}`);
    }

    const data: VideoDataResponse = await result.json();
    return data.items;
};

export const SearchVideoWithAnalysis = async (): Promise<VideoWithAnalysisDTO[]> => {
    const url = "https://localhost:7138/api/videos/ai";

    const result = await fetch(url);
    if(!result.ok){
        throw new Error(`API error ${result.status}`);
    }

    const data: VideoWithAnalysisResponse = await result.json();
    return data.items;
}