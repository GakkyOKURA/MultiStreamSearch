//共通の動画再生ページ
import { useParams } from "react-router-dom";
import {
  useCurrentVideoStore,
  useLiveStreamStore,
  useShortVideoStore,
} from "../store/videoStore";
import type { UnifiedVideo } from "../components/unified/unifiedVideo";
import { AspectRatio, Box, Flex, Text } from "@chakra-ui/react";
import { UnifiedVideoCard } from "../components/unified/unifiedVideoCard";

const VideoPage = () => {
  const { platform, id } = useParams();
  if (!platform || !id) {
    return;
  }

  const liveStreamResult = useLiveStreamStore((s) => s.results);
  const shortVideoResult = useShortVideoStore((s) => s.results);
  const paramVideo =
    platform === "youtubeLiveStream" || platform === "twitchStream"
      ? liveStreamResult
      : shortVideoResult;

  const currentVideo = useCurrentVideoStore((s) => s.current);
  if (!currentVideo) {
    return;
  }

  return (
    <Flex height="100vh">
      {/* 左：プレイヤー */}{" "}
      <Box flex="7" pl="40px" py="20px" pr="15px">
        <VideoPlayer platform={platform} id={id} />

        <Box mb="16px">
          <Text fontSize="2xl" fontWeight="bold">
            {currentVideo.title}
          </Text>

          <Text fontSize="md" color="gray.500">
            {currentVideo.channelName}
          </Text>
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

// export const VideoList = ({ videos }: { videos: UnifiedVideo[] }) => {
//   return (
//     <div style={{ overflowY: "auto", height: "100%" }}>
//       {videos.map((v) => (
//         <UnifiedVideoCard key={v.id} item={v} />
//       ))}
//     </div>
//   );
// };

// import { useEffect, useRef } from "react";

// export const VideoList = ({ videos }: { videos: UnifiedVideo[] }) => {
//   const current = useCurrentVideoStore((s) => s.current);
//   if (!current) {
//     return;
//   }
//   const currentRef = useRef<HTMLDivElement | null>(null);

//   useEffect(() => {
//     if (currentRef.current) {
//       currentRef.current.scrollIntoView({
//         behavior: "auto",
//         block: "center",
//       });
//     }
//   }, [current]);

//   return (
//     <div style={{ overflowY: "auto", height: "100%" }}>
//       {videos.map((v) => (
//         <div key={v.id} ref={v.id === current.id ? currentRef : null}>
//           <UnifiedVideoCard item={v} />
//         </div>
//       ))}
//     </div>
//   );
// };

import { useEffect, useRef } from "react";

export const VideoList = ({ videos }: { videos: UnifiedVideo[] }) => {
  const current = useCurrentVideoStore((s) => s.current);
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
        const isCurrent = v.id === current.id;

        return (
          <div
            key={v.id}
            ref={isCurrent ? currentRef : null}
            style={{
              background: isCurrent ? "#bcbcbc" : "transparent",
              pointerEvents: isCurrent ? "none" : "auto",
              opacity: isCurrent ? 1 : 1,
            }}
          >
            <UnifiedVideoCard item={v} />
          </div>
        );
      })}
    </div>
  );
};

// export const VideoList = ({ videos }: { videos: UnifiedVideo[] }) => {
//   return (
//     <div style={{ overflowY: "auto", height: "100%" }}>
//       {videos.map((v) => (
//         <div

//           key={v.id}
//           //onClick={() => onSelect(v)}
//           style={{ padding: "8px", cursor: "pointer" }}
//         >
//           {v.title}
//         </div>
//       ))}
//     </div>
//   );
// };
