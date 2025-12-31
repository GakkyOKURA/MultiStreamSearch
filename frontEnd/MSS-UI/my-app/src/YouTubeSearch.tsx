// import { useState } from "react";
// import { Box, Input, Button, VStack, Text } from "@chakra-ui/react";

// export const YouTubeSearch = () => {
//   const [keyword, setKeyword] = useState("");
//   const [result, setResult] = useState<any[]>([]);
//   const [loading, setLoading] = useState(false);

//   const handleSearch = async () => {
//     if (!keyword.trim()) return;

//     setLoading(true);

//     try {
//       const res = await fetch(
//         `http://localhost:5000/api/youtube/search/details?keyword=${encodeURIComponent(
//           keyword
//         )}`
//       );

//       const data = await res.json();
//       setResult(data);
//     } catch (err) {
//       console.error(err);
//     } finally {
//       setLoading(false);
//     }
//   };

//   return (
//     <VStack spacing={4} align="stretch">
//       <Input
//         placeholder="検索ワードを入力"
//         value={keyword}
//         onChange={(e) => setKeyword(e.target.value)}
//       />

//       <Button
//         colorScheme="blue"
//         onClick={handleSearch}
//         isLoading={loading}
//       >S
//         YouTube を検索
//       </Button>

//       <Box>
//         {result.map((item) => (
//           <Box key={item.videoId} p={3} borderWidth="1px" borderRadius="md" mb={2}>
//             <Text fontWeight="bold">{item.title}</Text>
//             <Text fontSize="sm" color="gray.500">
//               {item.channelTitle} / {item.viewCount} views
//             </Text>
//           </Box>
//         ))}
//       </Box>
//     </VStack>
//   );
// };
