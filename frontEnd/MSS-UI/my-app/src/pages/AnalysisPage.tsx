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
} from "@chakra-ui/react";

import { useVideoWithAnalysisStore } from "../store/videoStore";
import { SearchVideoWithAnalysis } from "../components/videos/searchVideoData";
import type { VideoDataDTO } from "../components/videos/videoData";
import VideoWithAnalysisCard from "../components/videos/videoWithACard";

export default function SearchAnalysisPage() {
  //const [videoData, setVideoData] = useState<VideoDataDTO[]>([]);

  const setVideoWithAnalysisDataResults = useVideoWithAnalysisStore(
    (s) => s.setResults,
  );
  const videoDataResults = useVideoWithAnalysisStore((s) => s.results);

  useEffect(() => {
    const load = async () => {
      const data = await SearchVideoWithAnalysis();
      setVideoWithAnalysisDataResults(data);
      console.log("hello");
    };
    load();
  }, []); // 空配列で初回だけ実行の合図

  return (
    <div style={{ maxWidth: "80%", margin: "0 auto", padding: "20px" }}>
      <SimpleGrid columns={{ base: 1, md: 1 }} gap="16px">
        {videoDataResults.map((item) => (
          <VideoWithAnalysisCard key={item.videoId} item={item} />
        ))}
      </SimpleGrid>
    </div>
  );
}
