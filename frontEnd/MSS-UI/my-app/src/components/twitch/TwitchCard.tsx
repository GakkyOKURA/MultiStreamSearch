import { Link } from "react-router-dom";
import type { UnifiedVideo } from "../unified/unifiedVideo";

export default function TwitchCard({ item }: { item: UnifiedVideo }) {
  const link =
    item.type === "twitchLive"
      ? `/video/twitch/${item.id}`
      : `/video/twitchClip/${item.id}`;

  return (
    <Link
      to={link}
      style={{
        textDecoration: "none",
        color: "inherit",
      }}
    >
      <div
        style={{
          display: "flex",
          gap: "12px",
          padding: "12px",
          borderBottom: "1px solid #ddd",
          cursor: "pointer",
        }}
      >
        <img
          src={item.thumbnailUrl}
          alt={item.title}
          style={{ width: "200px", borderRadius: "8px" }}
        />

        <div>
          <h3 style={{ margin: "0 0 8px 0" }}>
            {item.channelName}

            {item.type === "twitchLive" && (
              <span
                style={{
                  marginLeft: "8px",
                  color: "red",
                  fontSize: "14px",
                  fontWeight: "bold",
                }}
              >
                ● LIVE
              </span>
            )}
          </h3>

          <p style={{ margin: 0, color: "#555" }}>{item.title}</p>

          {item.viewerCount !== undefined && (
            <p style={{ marginTop: "4px", color: "#999", fontSize: "12px" }}>
              👁 {item.viewerCount} viewers
            </p>
          )}
        </div>
      </div>
    </Link>
  );
}
