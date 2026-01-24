export type TwitchStreamSearchDto = {
    id: string;
    userId: string;
    userLogin: string;
    userName: string;
    gameId: string;
    gameName: string;
    type: string;
    title: string;
    thumbnailUrl: string;
};

export type TwitchStreamSearchResult = {
    data: TwitchStreamSearchDto[];
};


export type TwitchClipSearchDto = {
    id: string;
    url: string;
    embedUrl: string;
    broadcasterId: string;
    broadcasterName: string;
    videoId: string;
    title: string;
    thumbnailUrl: string;
};

export type TwitchClipSearchResult = {
    data: TwitchClipSearchDto[];
};