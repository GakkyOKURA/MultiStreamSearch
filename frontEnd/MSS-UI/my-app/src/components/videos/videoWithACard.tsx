import { Link } from "react-router-dom";
import { Box, Flex, Image, Separator, Text } from "@chakra-ui/react";
import type { VideoWithAnalysisDTO } from "./videoWithAnalysis";
import type { Platform } from "./platform";
import { useCurrentVideoDataStore } from "../../store/videoStore";

export default function VideoWithAnalysisCard({
  item,
}: {
  item: VideoWithAnalysisDTO;
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
      <Box _hover={{ bg: "gray.200" }}>
        <Flex gap="12px" p="12px" cursor="pointer" maxH="300px">
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
            width="400px"
            borderRadius="8px"
            objectFit="cover"
            flexShrink={1} // ← 画像は縮んでOK
          />

          <Box
            flex="1"
            minWidth="0" // ← 折り返し可能にする
            flexShrink={1} // ← テキストは縮ませない
          >
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
                boxSize="60px"
                borderRadius="full"
                flexShrink={0} // ← アイコンが潰れないように
              />

              {/* チャンネル名（1行制限） */}
              <Text
                fontSize="sm"
                color="gray.500"
                lineClamp={1}
                marginTop="20px"
                marginLeft="10px"
              >
                {item.channelName}
              </Text>
            </Flex>
            {/* <Image src={item.bannerExternalUrl} width="800"></Image> */}

            <Flex align="flex-start" gap={3}>
              <Box
                marginTop={5}
                bg="purple.600"
                color="white"
                p={3}
                borderRadius="lg"
                position="relative"
                //maxW="200px" // 幅を制限するとわかりやすい
                wordBreak="break-word"
                _after={{
                  content: '""',
                  position: "absolute",
                  top: "-10px",
                  left: "15px",
                  borderLeft: "9px solid transparent",
                  borderRight: "2px solid transparent",
                  borderBottom: "13px solid",
                  borderBottomColor: "purple.600", // Chakraのテーマカラーを確実に当てる
                }}
              >
                <Text lineClamp={3}>{item.channelDescription}</Text>
              </Box>
            </Flex>
          </Box>
        </Flex>
        <Box
          marginRight={3}
          marginLeft={3}
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
        <Separator my={3} borderColor="gray.500" borderWidth="1px" />
      </Box>
    </Link>
  );
}
