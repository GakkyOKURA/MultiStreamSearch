import { useState, useEffect } from "react";
import {
  Tabs,
  TabsList,
  TabsTrigger,
  TabsContent,
  Flex,
  Box,
  Text,
  Image,
  SimpleGrid,
  Button,
} from "@chakra-ui/react";

import { useVideoDataStore } from "../store/videoStore";
import { SearchVideoData } from "../components/videos/searchVideoData";
import type { VideoDataDTO } from "../components/videos/videoData";
import VideoCard from "../components/videos/videoCard";
import { Link } from "react-router-dom";

export default function SearchPage() {
  const [videoData, setVideoData] = useState<VideoDataDTO[]>([]);

  const setVideoDataResults = useVideoDataStore((s) => s.setResults);
  const videoDataResults = useVideoDataStore((s) => s.results);

  useEffect(() => {
    const load = async () => {
      const data = await SearchVideoData();
      setVideoDataResults(data);
      console.log("hello");
    };
    load();
  }, []); // 空配列で初回だけ実行の合図

  return (
    <div style={{ maxWidth: "80%", margin: "0 auto", padding: "20px" }}>
      <Link to="/analysis">
        <Button colorScheme="orange">プロフィールへ</Button>
      </Link>

      <SimpleGrid columns={{ base: 1, md: 1 }} gap="16px">
        {videoDataResults.map((item) => (
          <VideoCard key={item.videoId} item={item} />
        ))}
      </SimpleGrid>
    </div>
  );
}
