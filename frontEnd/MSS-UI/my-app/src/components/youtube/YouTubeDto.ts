export type YouTubeSearchResult = {
    items: YouTubeSearchItemDto[];
};

export type YouTubeSearchItemDto = {
    id: YouTubeSearchItemIdDto;
    snippet: YouTubeSnippetDto;
};

export type YouTubeSearchItemIdDto = {
    videoId: string;
    channelId: string;
};

export type YouTubeSnippetDto = {
    channelId: string;
    title: string;
    description: string;
    thumbnails: YouTubeThumbnailsDto;
    channelTitle: string;
};

export type YouTubeThumbnailsDto = {
    medium: YouTubeThumbnailDto;
};

export type YouTubeThumbnailDto = {
    url: string;
    width: number;
    height: number;
};