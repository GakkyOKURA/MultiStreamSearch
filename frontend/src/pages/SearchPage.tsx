import { useEffect, useRef } from "react";
import {
  Box,
  SimpleGrid,
  IconButton,
  Image,
  Tooltip,
  MenuRoot,
  MenuTrigger,
  MenuContent,
  MenuItem,
  Flex,
} from "@chakra-ui/react";

import { useVideoDataStore, useNeedReloadStore } from "../store/videoStore";
import { SearchVideoData } from "../components/videos/searchVideoData";
import VideoCard from "../components/videos/videoCard";
import { Link } from "react-router-dom";

import vindiesIcon from "../assets/Vindies_Icon2.png";
import robotIcon from "../assets/robotIcon.png";
import shuffleIcon from "../assets/shuffle.png";
import bookIcon from "../assets/bookIcon.png";

const SearchPage = () => {
  const videoRefs = useRef<(HTMLDivElement | null)[]>([]);

  const setVideoDataResults = useVideoDataStore((s) => s.setResults);
  const videoDataResults = useVideoDataStore((s) => s.results);

  useEffect(() => {
    const load = async () => {
      var reloadNeeded = useNeedReloadStore.getState().isReloadNeeded;
      // false の場合 = 動画再生ページから戻ってきたとき
      // かつ、念のため videoDataResult のカウントもチェック
      if (!reloadNeeded && videoDataResults.length != 0) {
        // 値のリセット
        useNeedReloadStore.getState().setIsReloadNeeded(true);
        return;
      }

      // リロードの時、ページ初期表示の時のみ動画リスト更新
      const data = await SearchVideoData();
      setVideoDataResults(data);

      window.scrollTo({
        top: 0,
        behavior: "instant",
      });
    };
    load();
  }, []); // 空配列で初回だけ実行の合図

  const randomChoose = () => {
    if (videoDataResults.length === 0) {
      return;
    }
    const randomIndex = Math.floor(Math.random() * videoDataResults.length);
    const targetVideo = videoRefs.current[randomIndex];
    if (targetVideo) {
      targetVideo.scrollIntoView({ behavior: "smooth", block: "center" });
      const link = targetVideo.querySelector("a") as HTMLElement;
      if (link) {
        setTimeout(() => {
          link.click();
        }, 2000);
      }
    }
  };

  return (
    <div>
      <Box
        position={"fixed"}
        background={"whiteAlpha.950"}
        width={"100%"}
        zIndex={"sticky"}
        height={"60px"}
        alignItems="center"
        display="flex"
        justifyContent="flex-start"
      >
        <IconButton variant={"plain"} marginLeft={"10"}>
          <Image src={vindiesIcon} borderRadius="full" boxSize={"50px"} />

          <Link
            to="/analysis"
            onClick={() =>
              useNeedReloadStore.getState().setIsReloadNeeded(false)
            }
          >
            <Tooltip.Root>
              <Tooltip.Trigger asChild>
                <Image
                  src={robotIcon}
                  boxSize={"40px"}
                  marginBottom={1}
                  marginLeft={2}
                />
              </Tooltip.Trigger>
              <Tooltip.Positioner>
                <Tooltip.Content>
                  {"AI ランダム紹介"}
                  <Tooltip.Arrow />
                </Tooltip.Content>
              </Tooltip.Positioner>
            </Tooltip.Root>
          </Link>

          <Tooltip.Root>
            <Tooltip.Trigger asChild>
              <Image
                src={shuffleIcon}
                boxSize={"40px"}
                onClick={randomChoose}
                marginLeft={2}
              />
            </Tooltip.Trigger>
            <Tooltip.Positioner>
              <Tooltip.Content>
                {"ランダム Pick"}
                <Tooltip.Arrow />
              </Tooltip.Content>
            </Tooltip.Positioner>
          </Tooltip.Root>

          <MenuRoot>
            <MenuTrigger asChild>
              <Image src={bookIcon} boxSize={"40px"} />
            </MenuTrigger>

            <MenuContent>
              <MenuItem value="terms" asChild>
                <Link
                  to="/termsOfService"
                  onClick={() =>
                    useNeedReloadStore.getState().setIsReloadNeeded(false)
                  }
                >
                  利用規約
                </Link>
              </MenuItem>

              <MenuItem value="privacy" asChild>
                <Link
                  to="/privacyPolicy"
                  onClick={() =>
                    useNeedReloadStore.getState().setIsReloadNeeded(false)
                  }
                >
                  プライバシーポリシー
                </Link>
              </MenuItem>
            </MenuContent>
          </MenuRoot>
        </IconButton>
      </Box>

      <Flex
        direction="column"
        maxWidth={{ base: "100%", md: "90%" }}
        margin="0 auto"
        padding="20px"
      >
        <SimpleGrid columns={{ base: 1, md: 1 }} gap="16px" marginTop={"60px"}>
          {videoDataResults.map((item, index) => (
            <Box
              key={item.videoId}
              ref={(el: HTMLDivElement) => (videoRefs.current[index] = el)}
            >
              <VideoCard key={item.videoId} item={item} />
            </Box>
          ))}
        </SimpleGrid>
      </Flex>
    </div>
  );
};

export default SearchPage;
