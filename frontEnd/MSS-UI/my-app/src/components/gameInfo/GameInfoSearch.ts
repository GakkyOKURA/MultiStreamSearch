import type { GameInfoResult } from "./GameInfoDto"
import type { GameInfoDto } from "./GameInfoDto"

export const SearchGameInfo = async (): Promise<GameInfoDto[]> => {
    const url = "https://localhost:7138/api/gameinfos";

    const result = await fetch(url);
    if(!result.ok){
        throw new Error(`API error ${result.status}`);
    }

    const data: GameInfoResult = await result.json();
    return data.gameInfos;
};