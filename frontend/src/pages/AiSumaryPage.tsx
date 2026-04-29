import { useEffect } from "react";
import { Flex, SimpleGrid } from "@chakra-ui/react";

import { useVideoWithAnalysisStore } from "../store/videoStore";
import { SearchVideoWithAnalysis } from "../components/videos/searchVideoData";
import VideoWithAnalysisCard from "../components/videos/videoWithSummaryCard";
import { CommonHeader } from "../components/common/commonHeader";

const SearchSummaryPage = () => {
  const setVideoWithAnalysisDataResults = useVideoWithAnalysisStore(
    (s) => s.setResults,
  );
  const videoDataResults = useVideoWithAnalysisStore((s) => s.results);

  useEffect(() => {
    const load = async () => {
      const data = await SearchVideoWithAnalysis();
      setVideoWithAnalysisDataResults(data);
    };
    load();
  }, []); // 空配列で初回だけ実行の合図

  return (
    <div>
      <CommonHeader />
      <Flex
        direction="column"
        maxWidth={{ base: "100%", md: "90%" }}
        margin="0 auto"
        padding="20px"
      >
        <SimpleGrid gap="16px" marginTop={"60px"}>
          {videoDataResults.map((item) => (
            <VideoWithAnalysisCard key={item.videoId} item={item} />
          ))}
        </SimpleGrid>
      </Flex>
    </div>
  );
};

export default SearchSummaryPage;
