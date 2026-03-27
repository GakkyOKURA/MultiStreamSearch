//共通の動画再生ページ
import { useParams } from "react-router-dom";
import { useVideoDataStore } from "../store/videoStore";
import { Box, Flex, Image, Separator, Text } from "@chakra-ui/react";
import {
  VideoList,
  VideoPlayer,
} from "../components/videos/videoPageComponents";
import { CommonHeader } from "../components/common/commonHeader";
import { SearchVideoData } from "../components/videos/searchVideoData";
import { useEffect, useState } from "react";

const VideoPage = () => {
  // 1. Hooks は関数のトップレベルで呼ぶ
  const { platform, id } = useParams();
  const results = useVideoDataStore((s) => s.results);
  const setResults = useVideoDataStore((s) => s.setResults);

  // ローディング状態を管理するフラグ
  const [isLoading, setIsLoading] = useState(false);

  // videoStore が空の場合は useeffect で再レンダリング
  useEffect(() => {
    const fetchData = async () => {
      // ストアが空の場合のみ、バックエンドから取得
      if (results.length === 0) {
        setIsLoading(true);
        const data = await SearchVideoData();
        setResults(data);
        setIsLoading(false);
      }
    };

    fetchData();
  }, [results.length, setResults]);

  // 3. パラメータがない場合やローディング中の早期リターン
  if (!platform || !id) {
    return null;
  }

  if (isLoading) {
    return <div>ロード中...</div>;
  }

  // 4. 現在のビデオを特定する（results が更新されたら自動で再計算される）
  const currentVideo =
    platform === "youtubeLiveStream"
      ? results.find((v) => v.videoId === id)
      : results.find((v) => v.channelId === id);

  if (!currentVideo) {
    // データ取得後も見つからない場合
    if (!isLoading && results.length > 0) {
      return <div>Video not found.</div>;
    }
    return null;
  }

  return (
    <div>
      <CommonHeader />
      <Flex
        height={{ base: "none", md: "100vh" }}
        flexDirection={{ base: "column", md: "row" }}
        overflow={"hidden"}
        paddingTop={"60px"}
      >
        <Box
          flex="7"
          pl={{ base: "15px", md: "40px" }}
          pr="15px"
          overflowY="auto"
        >
          <VideoPlayer platform={platform} id={id} />
          <Box mb="16px">
            <Text fontSize="2xl" fontWeight="bold">
              {currentVideo.videoTitle}
            </Text>
            <Flex>
              <Image
                src={currentVideo.channelHighThumbnail.url}
                alt={currentVideo.channelName}
                boxSize="80px"
                borderRadius="full"
                flexShrink={0}
              />
              <Text
                fontSize="medium"
                marginTop="30px"
                marginLeft="20px"
                fontWeight="bold"
              >
                {currentVideo.channelName}
              </Text>
            </Flex>
            <Separator my={3} borderColor="gray.500" borderWidth="1px" />
            <Text fontSize="md" whiteSpace={"pre-wrap"}>
              {currentVideo.channelDescription}
            </Text>
          </Box>
        </Box>

        <Box
          height={{ base: "300px", md: "100%" }}
          alignSelf="center"
          maxWidth={{ base: "80%", md: "none" }}
          flex="3"
          overflowY="auto"
          minH={{ base: "700px", md: "auto" }}
          maxH={{ base: "700px", md: "100%" }}
        >
          <VideoList videos={results} platform={platform} id={id} />
        </Box>
      </Flex>
    </div>
  );
};

export default VideoPage;
