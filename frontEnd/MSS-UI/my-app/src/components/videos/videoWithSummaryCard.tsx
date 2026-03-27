import { Link } from "react-router-dom";
import { Box, Flex, Image, Separator, Text } from "@chakra-ui/react";
import type { VideoWithSummaryDTO } from "./videoWithSummary";
import type { Platform } from "./platform";
import { useCurrentVideoDataStore } from "../../store/videoStore";

export default function VideoWithAnalysisCard({
  item,
}: {
  item: VideoWithSummaryDTO;
}) {
  const Platform_CONFIG: Record<Platform, { link: string }> = {
    YouTube: { link: `/video/youtubeLiveStream/${item.videoId}` },
    Twitch: { link: `/video/twitchStream/${item.channelId}` },
  };

  return (
    <Link
      to={Platform_CONFIG[item.platform].link}
      onClick={() => useCurrentVideoDataStore.getState().setCurrent(item)}
      style={{ textDecoration: "none", color: "inherit" }}
    >
      <Box _hover={{ bg: "gray.200" }} p="12px">
        <Flex
          gap="12px"
          //p="12px"
          cursor="pointer"
          //maxH={{ base: "none", md: "300px" }}
          flexDirection={{ base: "column", md: "row" }}
        >
          <Box flex={"2"}>
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
                boxSize={{ base: "40px", md: "60px" }}
                borderRadius="full"
              />

              {/* チャンネル名（1行制限） */}
              <Text
                fontSize="sm"
                color="gray.500"
                lineClamp={1}
                marginTop={{ base: "10px", md: "20px" }}
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
                  left: { base: "14px", md: "15px" },
                  borderLeft: {
                    base: "6px solid transparent",
                    md: "9px solid transparent",
                  },
                  borderRight: {
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
        <Box
          // marginRight={3}
          // marginLeft={3}
          marginTop={3}
          bgImage={`url('${item.bannerExternalUrl}')`}
          bgSize="contain"
          bgColor="gray.400"
          p={3}
          borderRadius="lg"
        >
          <Text
            color="white"
            fontWeight="bold"
            textShadow="
          /* 1. カッチリした2pxの縁取り（12方向） */
          2px 2px 0 #000, -2px 2px 0 #000, 2px -2px 0 #000, -2px -2px 0 #000,
          2px 0px 0 #000, -2px 0px 0 #000, 0px 2px 0 #000, 0px -2px 0 #000,
          1px 2px 0 #000, -1px 2px 0 #000, 1px -2px 0 #000, -1px -2px 0 #000,
          /* 2. 外側に広がる強力な黒い霧（ぼかしを重ねて濃くする） */
          0 0 8px #000, 0 0 12px #000
          "
          >
            {item.aiDescription}
          </Text>
        </Box>
      </Box>
      <Separator my={3} borderColor="gray.500" borderWidth="1px" />
    </Link>
  );
}
