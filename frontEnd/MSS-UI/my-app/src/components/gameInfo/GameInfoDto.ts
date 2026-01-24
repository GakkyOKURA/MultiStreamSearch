export type GameInfoResult = {
    gameInfos: GameInfoDto[];
};

export type GameInfoDto = {
    gameName: string;
    gameId: string;
    boxArtTemplateUrl: string;
};