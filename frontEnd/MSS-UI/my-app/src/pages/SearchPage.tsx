import { UnifiedVideoCard } from "../components/unified/unifiedVideoCard";
import { useState, useEffect } from "react";
import {
  mergeAlternating,
  type UnifiedVideo,
} from "../components/unified/unifiedVideo";
import {
  Tabs,
  TabsList,
  TabsTrigger,
  TabsContent,
  Flex,
  Box,
  Text,
  Image,
  SimpleGrid,
} from "@chakra-ui/react";
import { searchYouTubeLiveStream } from "../components/youtube/YouTubeSearch";
import { searchYouTubeShort } from "../components/youtube/YouTubeSearch";
import { searchTwitchStream } from "../components/twitch/TwitchSearch";
import { searchTwitchClip } from "../components/twitch/TwitchSearch";
import { createBoxArtUrl } from "../Utils/boxArtUrl";
import { SearchGameInfo } from "../components/gameInfo/GameInfoSearch";

import {
  mapShortToUnified,
  mapYouTubeToUnified,
} from "../components/unified/youtubeMapper";
import {
  mapTwitchClipToUnified,
  mapTwitchStreamToUnified,
} from "../components/unified/twitchMapper";
import type { GameInfoDto } from "../components/gameInfo/GameInfoDto";
import type { YouTubeSearchItemDto } from "../components/youtube/YouTubeDto";
import type {
  TwitchClipSearchDto,
  TwitchStreamSearchDto,
} from "../components/twitch/TwitchDto";

import { useLiveStreamStore } from "../store/videoStore";
import { useShortVideoStore } from "../store/videoStore";
import { searchLiveStream } from "../components/unified/setUnifiedVideo";
import { searchSnap } from "../components/unified/setUnifiedVideo";

export default function SearchPage() {
  // UnifiedVideo
  //const [streamUnifiedResult, setUnifiedResults] = useState<UnifiedVideo[]>([]);
  // UnifiedVideo(snap)
  // const [snapUnifiedResult, setSnapUnifiedResults] = useState<UnifiedVideo[]>(
  //   []
  // );
  const [gameInfos, setGameInfos] = useState<GameInfoDto[]>([]);

  const setLiveStreamResults = useLiveStreamStore((s) => s.setResults);
  const liveStreamResults = useLiveStreamStore((s) => s.results);

  const setShortVideoResults = useShortVideoStore((s) => s.setResults);
  const shortVideoresults = useShortVideoStore((s) => s.results);

  // const searchLiveStream = async (
  //   gameName: string,
  //   gameId: string
  // ): Promise<void> => {
  //   const [youtube, twitch] = await Promise.all([
  //     searchYouTubeLiveStream(gameName),
  //     searchTwitchStream(gameId),
  //   ]);

  //   if (!youtube || !twitch) {
  //     return;
  //   }
  //   mapAndMergeLive([youtube, twitch]);
  // };

  // const searchSnap = async (
  //   gameName: string,
  //   gameId: string
  // ): Promise<void> => {
  //   const [short, clip] = await Promise.all([
  //     searchYouTubeShort(gameName),
  //     searchTwitchClip(gameId),
  //   ]);

  //   if (!short || !clip) {
  //     return;
  //   }
  //   mapAndMergeSnap([short, clip]);
  // };

  // const mapAndMergeLive = ([youtube, twitch]: [
  //   YouTubeSearchItemDto[],
  //   TwitchStreamSearchDto[]
  // ]): void => {
  //   const youtubeUnified = youtube.map(mapYouTubeToUnified);
  //   const twitchUnified = twitch.map(mapTwitchStreamToUnified);

  //   const merged = mergeAlternating(youtubeUnified, twitchUnified);
  //   setUnifiedResults(merged);
  //   setResults(merged); //← Zustand に保存
  // };

  // const mapAndMergeSnap = ([short, clip]: [
  //   YouTubeSearchItemDto[],
  //   TwitchClipSearchDto[]
  // ]): void => {
  //   const shortUnified = short.map(mapShortToUnified);
  //   const clipUnified = clip.map(mapTwitchClipToUnified);

  //   const merged = mergeAlternating(shortUnified, clipUnified);
  //   setSnapUnifiedResults(merged);
  // };

  // インクリメンタルにカテゴリ検索
  // useEffect(() => {
  //   if (!debouncedQuery) {
  //     setCategories([]);
  //     return;
  //   }
  //   fetchCategories();
  // }, [debouncedQuery]);

  // const fetchCategories = async () => {
  //   const res = await fetch(
  //     `https://localhost:7138/api/twitch/categories?query=${debouncedQuery}`
  //   );
  //   const data = await res.json();
  //   setCategories(data.data || []);
  // };

  // useEffect(() => {
  //   if (!selectedCategoryId) {
  //     return;
  //   }

  //   setLiveSearchDone(false);
  //   setSnapSearchDone(false);
  //   console.log("categories are selected  :" + selectedCategoryId);

  //   if (currentTab === "live") {
  //     searchLive();
  //   } else {
  //     searchSnap();
  //   }
  // }, [selectedCategoryId]);

  useEffect(() => {
    const load = async () => {
      const infos = await SearchGameInfo();
      setGameInfos(infos);
    };
    load();
  }, []); // 空配列で初回だけ実行の合図

  return (
    <div style={{ maxWidth: "80%", margin: "0 auto", padding: "20px" }}>
      {/* <div style={{ display: "flex", flexWrap: "wrap", gap: "16px" }}>
        {gameInfos.map((info) => (
          <div key={info.gameId}>
            <img
              src={createBoxArtUrl(info.gameId)}
              alt={info.gameName}
              width={150}
              height={200}
              onClick={() => {
                searchLiveStream(info.gameName, info.gameId);
                searchSnap(info.gameName, info.gameId);
              }}
            />
            <p>{info.gameName}</p>
          </div>
        ))}
      </div> */}

      <Flex wrap="wrap" gap="16px">
        {gameInfos.map((info) => (
          <Box key={info.gameId} cursor="pointer">
            <Image
              src={createBoxArtUrl(info.gameId)}
              alt={info.gameName}
              width="150px"
              height="200px"
              objectFit="cover"
              borderRadius="8px"
              onClick={() => {
                searchLiveStream(info.gameName, info.gameId);
                searchSnap(info.gameName, info.gameId);
              }}
              _hover={{ opacity: 0.8 }}
            />
            <Text
              mt="4px"
              fontSize="1xl"
              textAlign="center"
              fontWeight="bold"
              lineClamp={1}
            >
              {info.gameName}
            </Text>
          </Box>
        ))}
      </Flex>

      {/* ▼▼▼ ここから Tabs ▼▼▼ */}
      <Tabs.Root defaultValue="live">
        <TabsList>
          <TabsTrigger value="live">Live</TabsTrigger>
          <TabsTrigger value="snap">Snap</TabsTrigger>
        </TabsList>

        {/* Live */}
        <TabsContent value="live">
          <SimpleGrid columns={{ base: 1, md: 1 }} gap="16px">
            {liveStreamResults
              .filter(
                (item) =>
                  item.type === "twitchStream" ||
                  item.type === "youtubeLiveStream"
              )
              .map((item) => (
                <UnifiedVideoCard key={item.id} item={item} />
              ))}
          </SimpleGrid>
        </TabsContent>

        {/* Snap */}
        <TabsContent value="snap">
          <SimpleGrid columns={{ base: 1, md: 1 }} gap="16px">
            {shortVideoresults
              .filter(
                (item) =>
                  item.type === "youtubeShort" || item.type === "twitchClip"
              )
              .map((item) => (
                <UnifiedVideoCard key={item.id} item={item} />
              ))}
          </SimpleGrid>
        </TabsContent>
      </Tabs.Root>
      {/* ▲▲▲ Tabs 終わり ▲▲▲ */}
    </div>
  );
}
