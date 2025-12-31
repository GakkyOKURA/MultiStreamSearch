import { Link } from "react-router-dom";

export default function TwitchClipCard({ item }: { item: any }) {
  // Streams API のサムネイルは {width} と {height} を置換する必要がある
  const thumb = item.thumbnail_url
    ?.replace("{width}", "200")
    ?.replace("{height}", "112");

  return (
    <Link
      to={`/video/twitchClip/${item.id}`}
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
          src={thumb}
          alt={item.broadcaster_name}
          style={{ width: "200px", borderRadius: "8px" }}
        />

        <div>
          <h3 style={{ margin: "0 0 8px 0" }}>
            {item.broadcaster_name}

            {item.type === "live" && (
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

          <p style={{ margin: 0, color: "#555" }}>{item.game_id}</p>

          <p style={{ marginTop: "8px", color: "#777", fontSize: "14px" }}>
            {item.title?.slice(0, 80)}...
          </p>

          <p style={{ marginTop: "4px", color: "#999", fontSize: "12px" }}>
            👁 {item.view_count} view
          </p>
        </div>
      </div>
    </Link>
  );
}
