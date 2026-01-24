import type { TwitchStreamSearchResult } from "./TwitchDto";
import type { TwitchStreamSearchDto } from "./TwitchDto";
import type { TwitchClipSearchResult } from "./TwitchDto";
import type { TwitchClipSearchDto } from "./TwitchDto";

// TwitchStream 検索
export const searchTwitchStream = async (
    selectedGameId: string
): Promise<TwitchStreamSearchDto[] | undefined>  => {
    if (!selectedGameId) {
      return;
    }

    const url = 
    `https://localhost:7138/api/twitch/streams?gameId=${selectedGameId}`;

    const result = await fetch(url);
    if(!result.ok){
        throw new Error(`API error ${result.status}`);
    }

    const data: TwitchStreamSearchResult = await result.json();
    return data.data
};

// TwitchClip 検索
export const searchTwitchClip = async (
    selectedGameId: string
): Promise<TwitchClipSearchDto[] | undefined> => {
    if (!selectedGameId) {
      return;
    }

    const url = 
    `https://localhost:7138/api/twitch/clips?gameId=${selectedGameId}`;

    const result = await fetch(url);
    if(!result.ok){
        throw new Error(`API error ${result.status}`);
    }

    const data: TwitchClipSearchResult = await result.json();
    return data.data;
};