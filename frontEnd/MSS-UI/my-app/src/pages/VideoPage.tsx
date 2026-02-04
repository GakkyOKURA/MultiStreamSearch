//共通の動画再生ページ
import { useParams } from "react-router-dom";
import {
  useCurrentVideoDataStore,
  useVideoDataStore,
} from "../store/videoStore";
import {
  AspectRatio,
  Box,
  Flex,
  Image,
  Separator,
  Text,
} from "@chakra-ui/react";

const VideoPage = () => {
  const { platform, id } = useParams();
  if (!platform || !id) {
    return;
  }

  const videoDataResult = useVideoDataStore((s) => s.results);
  const paramVideo = videoDataResult;

  const currentVideo = useCurrentVideoDataStore((s) => s.current);
  if (!currentVideo) {
    return;
  }

  return (
    <Flex height="100vh">
      {/* 左：プレイヤー */}
      <Box flex="7" pl="40px" py="20px" pr="15px">
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
              flexShrink={0} // ← アイコンが潰れないように
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

          <Text fontSize="md">{currentVideo.channelDescription}</Text>
        </Box>
      </Box>
      {/* 右：リスト */}
      <Box
        flex="3"
        borderLeft="1px solid #444"
        display={{ base: "none", md: "block" }}
      >
        <VideoList videos={paramVideo} />
      </Box>
    </Flex>
  );
};

export default VideoPage;

export const VideoPlayer = ({
  platform,
  id,
}: {
  platform: string;
  id: string;
}) => {
  if (!platform || !id) {
    return <div>Invalid video URL</div>;
  }

  if (platform === "youtubeLiveStream" || platform === "youtubeShort") {
    return (
      <AspectRatio ratio={16 / 9} width="100%">
        <iframe
          // width="80%"
          // height="500"
          src={`https://www.youtube.com/embed/${id}?autoplay=1&mute=0`}
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; keyboard-map"
          allowFullScreen
        />
      </AspectRatio>
    );
  }

  if (platform === "twitchStream") {
    return (
      <AspectRatio ratio={16 / 9} width="100%">
        <iframe
          src={`https://player.twitch.tv/?channel=${id}&parent=localhost&autoplay=true&muted=false`}
          // width="80%"
          // height="60%"
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; keyboard-map"
          allowFullScreen
        />
      </AspectRatio>
    );
  }

  if (platform === "twitchClip") {
    return (
      <AspectRatio ratio={16 / 9} width="100%">
        <iframe
          src={`https://clips.twitch.tv/embed?clip=${id}&parent=localhost&autoplay=true&muted=false`}
          // width="100%"
          // height="500"
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; keyboard-map"
          allowFullScreen
        />
      </AspectRatio>
    );
  }
};

import { useEffect, useRef } from "react";
import type { VideoDataDTO } from "../components/videos/videoData";
import VideoCard from "../components/videos/videoCard";

export const VideoList = ({ videos }: { videos: VideoDataDTO[] }) => {
  const current = useCurrentVideoDataStore((s) => s.current);
  if (!current) {
    return;
  }
  const currentRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (currentRef.current) {
      currentRef.current.scrollIntoView({
        behavior: "auto",
        block: "center",
      });
    }
  }, [current]);

  return (
    <div style={{ overflowY: "auto", height: "100%" }}>
      {videos.map((v) => {
        const isCurrent = v.videoId === current.videoId;

        return (
          <div
            key={v.videoId}
            ref={isCurrent ? currentRef : null}
            style={{
              background: isCurrent ? "#bcbcbc" : "transparent",
              pointerEvents: isCurrent ? "none" : "auto",
              opacity: isCurrent ? 1 : 1,
            }}
          >
            <VideoCard item={v} />
          </div>
        );
      })}
    </div>
  );
};
