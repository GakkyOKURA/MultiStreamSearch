// import { Link } from "react-router-dom";
// import type { UnifiedVideo } from "../unified/unifiedVideo";
// import { useCurrentVideoStore } from "../../store/videoStore";

// export default function TwitchCard({ item }: { item: UnifiedVideo }) {
//   const link =
//     item.type === "twitchStream"
//       ? `/video/twitchStream/${item.id}`
//       : `/video/twitchClip/${item.id}`;

//   return (
//     <Link
//       to={link}
//       style={{
//         textDecoration: "none",
//         color: "inherit",
//       }}
//       onClick={() => useCurrentVideoStore.getState().setCurrent(item)}
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
//           src={item.thumbnailUrl}
//           alt={item.title}
//           style={{ width: "200px", borderRadius: "8px" }}
//         />

//         <div>
//           <h3 style={{ margin: "0 0 8px 0" }}>
//             {item.channelName}

//             {item.type === "twitchStream" && (
//               <span
//                 style={{
//                   marginLeft: "8px",
//                   color: "red",
//                   fontSize: "14px",
//                   fontWeight: "bold",
//                 }}
//               >
//                 ● LIVE
//               </span>
//             )}
//           </h3>

//           <p style={{ margin: 0, color: "#555" }}>{item.title}</p>
//         </div>
//       </div>
//     </Link>
//   );
// }

// import { Link } from "react-router-dom";
// import { Box, Flex, Image, Text } from "@chakra-ui/react";
// import type { UnifiedVideo } from "../unified/unifiedVideo";
// import { useCurrentVideoStore } from "../../store/videoStore";

// export default function TwitchCard({ item }: { item: UnifiedVideo }) {
//   const link =
//     item.type === "twitchStream"
//       ? `/video/twitchStream/${item.id}`
//       : `/video/twitchClip/${item.id}`;

//   return (
//     <Link
//       to={link}
//       onClick={() => useCurrentVideoStore.getState().setCurrent(item)}
//       style={{ textDecoration: "none", color: "inherit" }}
//     >
//       <Flex
//         gap="12px"
//         p="12px"
//         borderBottom="1px solid #ddd"
//         cursor="pointer"
//         _hover={{ bg: "gray.200" }}
//       >
//         <Image
//           src={item.thumbnailUrl}
//           alt={item.title}
//           width="400px"
//           borderRadius="8px"
//           objectFit="cover"
//           flexShrink={1} // ← 画像は縮んでOK
//         />

//         <Box
//           flex="1"
//           minWidth="0" // ← 折り返し可能にする
//           flexShrink={0} // ← テキストは縮ませない
//         >
//           {/* タイトル（2行制限） */}
//           <Text fontSize="lg" fontWeight="bold" lineClamp={2}>
//             {item.title}

//             {item.type === "twitchStream" && (
//               <Text
//                 as="span"
//                 color="red"
//                 ml="8px"
//                 fontSize="sm"
//                 fontWeight="bold"
//               >
//                 ● LIVE
//               </Text>
//             )}
//           </Text>

//           {/* チャンネル名（1行制限） */}
//           <Text fontSize="sm" color="gray.500" lineClamp={1}>
//             {item.channelName}
//           </Text>
//         </Box>
//       </Flex>
//     </Link>
//   );
// }
