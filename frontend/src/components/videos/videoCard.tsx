import { Link } from "react-router-dom";
import { Box, Flex, Image, Separator, Text } from "@chakra-ui/react";
import type { VideoDataDTO } from "./videoData";
import type { Platform } from "./platform";
import {
  useCurrentVideoDataStore,
  useNeedReloadStore,
} from "../../store/videoStore";

export default function VideoCard({
  item,
  forceColumn = false, // 縦表示するときは true。モバイルと、 VideoPage のリストの時。
}: {
  item: VideoDataDTO;
  forceColumn?: boolean;
}) {
  const Platform_CONFIG: Record<Platform, { link: string }> = {
    YouTube: { link: `/video/youtubeLiveStream/${item.videoId}` },
    Twitch: { link: `/video/twitchStream/${item.channelId}` },
  };

  return (
    <Link
      to={Platform_CONFIG[item.platform].link}
      onClick={() => {
        useCurrentVideoDataStore.getState().setCurrent(item);
        useNeedReloadStore.getState().setIsReloadNeeded(false);
      }}
      style={{ textDecoration: "none", color: "inherit" }}
    >
      <Flex
        flexDirection={forceColumn ? "column" : { base: "column", md: "row" }}
        gap="12px"
        p="12px"
        cursor="pointer"
        _hover={{ bg: "gray.200" }}
      >
        <Box flex="2">
          <Image
            src={item.searchHighTumbnail.url}
            onError={(e) => {
              e.currentTarget.src = `https://i.ytimg.com/vi/${item.videoId}/hqdefault.jpg`;
            }}
            onLoad={(e) => {
              const img = e.currentTarget;
              // 正常な hq720 なら naturalWidth は 1280 になるはず
              // グレー画像の場合は 120 など極端に小さい
              if (img.naturalWidth <= 120) {
                // グレー画像だと判定されたら、hqdefault に差し替える
                img.src = `https://i.ytimg.com/vi/${item.videoId}/hqdefault.jpg`;
              }
            }}
            alt={item.videoTitle}
            width="100%"
            borderRadius="8px"
            flexShrink={1} // ← 画像は縮んでOK
            loading="lazy"
          />
        </Box>

        <Box flex="3">
          {/* タイトル（2行制限） */}
          <Text fontSize="lg" fontWeight="bold" lineClamp={2}>
            {item.videoTitle}
          </Text>

          <Text fontSize="sm" lineClamp={2}>
            {item.searchDescription}
          </Text>

          <Flex>
            <Image
              src={item.channelHighThumbnail.url}
              alt={item.channelName}
              boxSize={forceColumn ? "40px" : { base: "40px", md: "60px" }}
              borderRadius="full"
              loading="lazy"
            />

            {/* チャンネル名（1行制限） */}
            <Text
              fontSize="sm"
              color="gray.500"
              lineClamp={1}
              marginTop={forceColumn ? "10px" : { base: "10px", md: "20px" }}
              marginLeft="10px"
            >
              {item.channelName}
            </Text>
          </Flex>

          <Flex align="flex-start" gap={3}>
            <Box
              visibility={item.channelDescription ? "visible" : "hidden"}
              minW="45px"
              marginTop={5}
              bg="purple.600"
              color="white"
              p={3}
              borderRadius="lg"
              position="relative"
              wordBreak="break-word"
              _after={{
                content: '""',
                position: "absolute",
                top: "-10px",
                left: forceColumn ? "14px" : { base: "14px", md: "15px" },
                borderLeft: forceColumn
                  ? "6px solid transparent"
                  : {
                      base: "6px solid transparent",
                      md: "9px solid transparent",
                    },
                borderRight: forceColumn
                  ? "6px solid transparent"
                  : {
                      base: "6px solid transparent",
                      md: "2px solid transparent",
                    },
                borderBottom: "13px solid",
                borderBottomColor: "purple.600",
              }}
            >
              <Text lineClamp={3}>{item.channelDescription}</Text>
            </Box>
          </Flex>
        </Box>
      </Flex>
      <Separator my={3} borderColor="gray.500" borderWidth="1px" />
    </Link>
  );
}
