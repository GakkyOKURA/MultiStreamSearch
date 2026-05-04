//共通の動画再生ページ
import { useParams } from "react-router-dom";
import { useVideoDataStore } from "../store/videoStore";
import {
  Box,
  Button,
  Flex,
  Image,
  Link,
  Separator,
  Text,
  useDisclosure,
} from "@chakra-ui/react";

import {
  VideoList,
  VideoPlayer,
} from "../components/videos/videoPageComponents";

import { CommonHeader } from "../components/common/commonHeader";
import { SearchVideoData } from "../components/videos/searchVideoData";
import { useEffect, useRef, useState } from "react";

const VideoPage = () => {
  // Hooks は関数のトップレベルで呼ぶ
  const { platform, id } = useParams();
  const results = useVideoDataStore((s) => s.results);
  const setResults = useVideoDataStore((s) => s.setResults);

  // ローディング状態を管理するフラグ
  const [isLoading, setIsLoading] = useState(false);

  const { open, onToggle } = useDisclosure();

  const listBoxRef = useRef<HTMLDivElement | null>(null);
  const scrollPositionRef = useRef<number>(0);

  const channelUrl =
    platform === "youtubeLiveStream"
      ? `https://www.youtube.com/channel/${id}`
      : `https://www.twitch.tv/${id}`;

  // videoStore が空の場合は useEffect で再レンダリング
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

  // 開く時にスクロール位置を復元
  useEffect(() => {
    if (open && listBoxRef.current) {
      listBoxRef.current.scrollTop = scrollPositionRef.current;
    }
  }, [open]);

  // パラメータがない場合やローディング中の早期リターン
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

  // 閉じる時にスクロール位置を保存
  const handleToggle = () => {
    if (open && listBoxRef.current) {
      scrollPositionRef.current = listBoxRef.current.scrollTop;
    }
    onToggle();
    if (!open) {
      // 直後だと dom が更新されてないので、少し待つ
      setTimeout(() => {
        window.scrollTo({
          top: document.body.scrollHeight,
          behavior: "auto",
        });
      }, 100);
    }
  };

  return (
    <div>
      <Box display={{ base: open ? "none" : "block", md: "block" }}>
        <CommonHeader />
      </Box>
      <Flex
        height={{ base: "none", md: "100vh" }}
        flexDirection={{ base: "column", md: "row" }}
        overflow={"hidden"}
        paddingTop={"60px"}
      >
        <Box
          flex="7"
          pl={{ base: "0px", md: "40px" }}
          pr={{ base: "0px", md: "15px" }}
          overflowY="auto"
        >
          <VideoPlayer platform={platform} id={id} />
          <Box
            mb="16px"
            pl={{ base: "15px", md: "0px" }}
            pr={{ base: "15px", md: "0px" }}
          >
            <Text fontSize="2xl" fontWeight="bold">
              {currentVideo.videoTitle}
            </Text>
            <Flex>
              <Link href={channelUrl} target="_blank">
                <Image
                  src={currentVideo.channelHighThumbnail.url}
                  alt={currentVideo.channelName}
                  boxSize="80px"
                  borderRadius="full"
                  flexShrink={0}
                />
              </Link>
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
          ref={listBoxRef}
          css={{
            overscrollBehavior: "contain",
            "@media (max-width: 768px)": {
              "&::-webkit-scrollbar": {
                display: "none",
              },
              scrollbarWidth: "none",
            },
          }}
          height={{ base: "300px", md: "100%" }}
          alignSelf="center"
          maxWidth={{ base: "100%", md: "none" }}
          flex="3"
          overflowY="auto"
          minH={{ base: "50px", md: "auto" }}
          maxH={{ base: "calc(100dvh - 10px)", md: "100%" }}
        >
          <Box
            position="sticky"
            top="0"
            zIndex="1"
            bg="white"
            display={{ base: "flex", md: "none" }}
            justifyContent="center"
          >
            <Button width="90%" onClick={handleToggle}>
              {open ? "動画リストを閉じる" : "動画リストを開く"}
            </Button>
          </Box>

          <Box
            visibility={{ base: open ? "visible" : "hidden", md: "visible" }}
            height={{ base: open ? "auto" : "0", md: "auto" }}
          >
            <VideoList
              videos={results}
              platform={platform}
              id={id}
              isVisible={open}
            />
          </Box>
        </Box>
      </Flex>
    </div>
  );
};

export default VideoPage;
