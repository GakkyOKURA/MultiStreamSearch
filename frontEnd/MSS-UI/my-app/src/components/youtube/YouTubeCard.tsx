// import { Link } from "react-router-dom";

// export default function YouTubeCard({ item }: { item: any }) {
//   const { snippet } = item;

//   const thumb =
//     snippet.thumbnails?.medium?.url || snippet.thumbnails?.default?.url;

//   return (
//     <Link
//       to={`/video/youtube/${item.id.videoId}`}
//       style={{
//         textDecoration: "none",
//         color: "inherit",
//       }}
//     >
//       <div
//         style={{
//           display: "flex",
//           gap: "12px",
//           padding: "12px",
//           borderBottom: "1px solid #ddd",
//           cursor: "pointer",
//         }}
//       >
//         <img
//           src={thumb}
//           alt={snippet.title}
//           style={{ width: "200px", borderRadius: "8px" }}
//         />

//         <div>
//           <h3 style={{ margin: "0 0 8px 0" }}>{snippet.title}</h3>
//           <p style={{ margin: 0, color: "#555" }}>{snippet.channelTitle}</p>
//           <p style={{ marginTop: "8px", color: "#777", fontSize: "14px" }}>
//             {snippet?.description?.slice(0, 80)}...
//           </p>
//         </div>
//       </div>
//     </Link>
//   );
// }

import { Link } from "react-router-dom";
import type { UnifiedVideo } from "../unified/unifiedVideo";

export default function YouTubeCard({ item }: { item: UnifiedVideo }) {
  return (
    <Link
      to={`/video/youtube/${item.id}`}
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
          <h3 style={{ margin: "0 0 8px 0" }}>{item.title}</h3>
          <p style={{ margin: 0, color: "#555" }}>{item.channelName}</p>

          {item.publishedAt && (
            <p style={{ marginTop: "8px", color: "#777", fontSize: "14px" }}>
              {new Date(item.publishedAt).toLocaleDateString()}
            </p>
          )}
        </div>
      </div>
    </Link>
  );
}
