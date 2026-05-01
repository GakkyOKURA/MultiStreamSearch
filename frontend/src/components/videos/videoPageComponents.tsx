import { AspectRatio } from "@chakra-ui/react";
import { useEffect, useRef } from "react";
import type { VideoDataDTO } from "./videoData";
import VideoCard from "./videoCard";

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

  // 開発時は env.development の値を使用
  const parent = import.meta.env.VITE_TWITCH_PARENT ?? "vindies.jp";

  if (platform === "youtubeLiveStream") {
    return (
      <AspectRatio ratio={16 / 9} width="100%">
        <iframe
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
          src={`https://player.twitch.tv/?channel=${id}&parent=${parent}&autoplay=true&muted=false`}
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; keyboard-map"
          allowFullScreen
        />
      </AspectRatio>
    );
  }
};

export const VideoList = ({
  videos,
  platform,
  id,
  isVisible,
}: {
  videos: VideoDataDTO[];
  platform: string;
  id: string;
  isVisible?: boolean;
}) => {
  const currentRef = useRef<HTMLDivElement | null>(null);

  const hasScrolled = useRef(false);

  useEffect(() => {
    // PC の場合（isVisible が undefined）は従来通り
    //if (isVisible === undefined) {
    if (currentRef.current) {
      currentRef.current.scrollIntoView({
        behavior: "auto",
        block: "center",
      });
    }

    // モバイルの場合
    if (hasScrolled.current) {
      return;
    }
    if (!isVisible) {
      window.scrollTo({ top: 0, behavior: "auto" });
      return;
    }

    if (currentRef.current) {
      // 直後だと dom が更新されてないので、少し待つ
      // 0 で問題ないことを確認。ただし setTimeout で囲うのは必須。
      setTimeout(() => {
        currentRef.current!.scrollIntoView({
          behavior: "auto",
          block: "center",
        });
      }, 0);
      hasScrolled.current = true;
      console.log("hellolll");
    }
  }, [isVisible]);

  return (
    <div style={{ overflowY: "auto", height: "100%" }}>
      {videos.map((v) => {
        const isCurrent =
          platform === "youtubeLiveStream"
            ? v.videoId === id
            : v.channelId === id;

        return (
          <div
            key={v.videoId}
            ref={isCurrent ? currentRef : null}
            style={{
              background: isCurrent ? "#bcbcbc" : "transparent",
              pointerEvents: isCurrent ? "none" : "auto",
            }}
          >
            <VideoCard item={v} forceColumn={true} />
          </div>
        );
      })}
    </div>
  );
};
