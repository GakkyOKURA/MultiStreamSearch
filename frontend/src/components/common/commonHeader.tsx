import {
  Box,
  IconButton,
  Image,
  MenuContent,
  MenuItem,
  MenuRoot,
  MenuTrigger,
  Tooltip,
} from "@chakra-ui/react";
import { Link } from "react-router-dom";
import { useNeedReloadStore } from "../../store/videoStore";
import vindiesIcon from "../../assets/Vindies_Icon2.png";
import bookIcon from "../../assets/bookIcon.png";
import questionIcon from "../../assets/QuestionIcon.png";

export const CommonHeader = () => {
  return (
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
        {/* SearchPage へ遷移、データの再取得 */}
        <Link
          to="/"
          onClick={() =>
            // ここでリロードのフラグをリセット。
            // Vindies のアイコンを押して SearchPage に戻った場合は、データを再取得させる
            useNeedReloadStore.getState().setIsReloadNeeded(true)
          }
        >
          <Image src={vindiesIcon} borderRadius="full" boxSize={"50px"} />
        </Link>

        {/* 利用規約、プライバシーポリシー */}
        <MenuRoot>
          <MenuTrigger asChild>
            <Image src={bookIcon} boxSize={"40px"} marginLeft={2} />
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

        {/* サイトについて */}
        <Link
          to="/howToUse"
          onClick={() =>
            // ここでリロード不要のフラグをセット
            useNeedReloadStore.getState().setIsReloadNeeded(false)
          }
        >
          <Tooltip.Root>
            <Tooltip.Trigger asChild>
              <Image src={questionIcon} boxSize={"40px"} marginLeft={2} />
            </Tooltip.Trigger>
            <Tooltip.Positioner>
              <Tooltip.Content>
                {"サイトについて"}
                <Tooltip.Arrow />
              </Tooltip.Content>
            </Tooltip.Positioner>
          </Tooltip.Root>
        </Link>
      </IconButton>
    </Box>
  );
};
