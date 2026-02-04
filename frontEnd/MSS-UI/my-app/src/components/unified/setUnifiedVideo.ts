// import { searchYouTubeLiveStream } from "../youtube/YouTubeSearch";
// import { searchYouTubeShort } from "../youtube/YouTubeSearch";
// import { searchTwitchStream } from "../twitch/TwitchSearch";
// import { searchTwitchClip } from "../twitch/TwitchSearch";
// import type { YouTubeSearchItemDto } from "../youtube/YouTubeDto";
// import type { TwitchStreamSearchDto } from "../twitch/TwitchDto";
// import type { TwitchClipSearchDto } from "../twitch/TwitchDto";
// import { mapYouTubeToUnified } from "./youtubeMapper";
// import { mapShortToUnified } from "./youtubeMapper";
// import { mapTwitchStreamToUnified } from "./twitchMapper";
// import { mapTwitchClipToUnified } from "./twitchMapper";
// import { mergeAlternating } from "./unifiedVideo";
// import { useLiveStreamStore } from "../../store/videoStore";
// import { useShortVideoStore } from "../../store/videoStore";

// export const searchLiveStream = async (
//     gameName: string,
//     gameId: string
// ): Promise<void> => {
//     const [youtube, twitch] = await Promise.all([
//     searchYouTubeLiveStream(gameName),
//     searchTwitchStream(gameId),
//     ]);

//     if (!youtube || !twitch) {
//     return;
//     }
//     mapAndMergeLive([youtube, twitch]);
// };

// export const searchSnap = async (
//     gameName: string,
//     gameId: string
// ): Promise<void> => {
//     const [short, clip] = await Promise.all([
//     searchYouTubeShort(gameName),
//     searchTwitchClip(gameId),
//     ]);

//     if (!short || !clip) {
//     return;
//     }
//     mapAndMergeSnap([short, clip]);
// };

// const mapAndMergeLive = ([youtube, twitch]: [
//     YouTubeSearchItemDto[],
//     TwitchStreamSearchDto[]
// ]): void => {
//     const youtubeUnified = youtube.map(mapYouTubeToUnified);
//     const twitchUnified = twitch.map(mapTwitchStreamToUnified);

//     const merged = mergeAlternating(youtubeUnified, twitchUnified);

//     useLiveStreamStore.getState().setResults(merged); //← Zustand に保存
// };

// const mapAndMergeSnap = ([short, clip]: [
//     YouTubeSearchItemDto[],
//     TwitchClipSearchDto[]
// ]): void => {
//     const shortUnified = short.map(mapShortToUnified);
//     const clipUnified = clip.map(mapTwitchClipToUnified);

//     const merged = mergeAlternating(shortUnified, clipUnified);

//     useShortVideoStore.getState().setResults(merged); //← Zustand に保存
// };