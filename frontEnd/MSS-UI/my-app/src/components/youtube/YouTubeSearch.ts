import type { YouTubeSearchResult } from "./YouTubeDto";
import type { YouTubeSearchItemDto } from "./YouTubeDto";

// YouTubeLiveStream 検索
export const searchYouTubeLiveStream = async (
    query: string
): Promise<YouTubeSearchItemDto[] | undefined> => {
    const url = 
    `https://localhost:7138/api/youtube/search?q=${query}`;

    const result = await fetch(url);
    if(!result.ok){
        throw new Error(`API error ${result.status}`);
    }

    const data: YouTubeSearchResult = await result.json();
    return data.items;
};


// YouTubeShort 検索
export const searchYouTubeShort = async (
    query: string
): Promise<YouTubeSearchItemDto[] | undefined> => {
    const url = 
    `https://localhost:7138/api/youtube/short?q=${query}`;

    const result = await fetch(url);
    if(!result.ok){
        throw new Error(`API error ${result.status}`);
    }

    const data: YouTubeSearchResult = await result.json();
    return data.items;
};