//共通の動画再生ページ
import { useParams } from "react-router-dom";

export default function VideoPage() {
  const { platform, id } = useParams();

  // どちらかが undefined の場合はエラーメッセージを返す
  if (!platform || !id) {
    return <div>Invalid video URL</div>;
  }

  if (platform === "youtube") {
    return (
      <iframe
        width="100%"
        height="500"
        src={`https://www.youtube.com/embed/${id}`}
        allowFullScreen
      />
    );
  }

  if (platform === "twitch") {
    return (
      <div>
        <div style={{ marginBottom: "10px" }}>
          <label>videoId: </label>
          <input
            type="text"
            value={`https://player.twitch.tv/?channel=${id}&parent=localhost`}
            readOnly
            style={{
              width: "300px",
              padding: "6px",
              border: "1px solid #ccc",
              borderRadius: "4px",
            }}
          />
        </div>

        <iframe
          src={`https://player.twitch.tv/?channel=${id}&parent=localhost`}
          // allow="autoplay; encrypted-media; fullscreen; picture-in-picture"
          width="100%"
          height="500"
          allowFullScreen
        />
      </div>
    );
  }

  if (platform === "twitchClip") {
    return (
      <div>
        <div style={{ marginBottom: "10px" }}>
          <label>videoId: </label>
          <input
            type="text"
            value={`https://clips.twitch.tv/embed?clip=${id}&parent=localhost`}
            readOnly
            style={{
              width: "300px",
              padding: "6px",
              border: "1px solid #ccc",
              borderRadius: "4px",
            }}
          />
        </div>

        <iframe
          src={`https://clips.twitch.tv/embed?clip=${id}&parent=localhost`}
          // allow="autoplay; encrypted-media; fullscreen; picture-in-picture"
          width="100%"
          height="500"
          allowFullScreen
        />
      </div>
    );
  }

  if (platform === "x") {
    return <div>Twitter動画は後で対応</div>;
  }

  return <div>Unknown platform</div>;
}
