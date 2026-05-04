import { Box, Flex, Image } from "@chakra-ui/react";
import ReactMarkDown from "react-markdown";
import { ChakraComponents } from "../components/common/ChakraComponents";
import { CommonHeader } from "../components/common/commonHeader";
import {
  HOWTOUSE1,
  HOWTOUSE2,
  HOWTOUSE3,
} from "../components/legalText/howToUse";

import robotIcon from "../assets/robotIcon.png";
import shuffleIcon from "../assets/shuffle.png";

const HowToUsePage = () => {
  const content1 = HOWTOUSE1;
  const content2 = HOWTOUSE2;
  const content3 = HOWTOUSE3;

  return (
    <div>
      <CommonHeader />
      <Box maxW={"3xl"} mx={"auto"} pt={8} pr={8} pl={8}>
        <ReactMarkDown components={ChakraComponents}>{content1}</ReactMarkDown>
      </Box>

      <Flex
        maxW={"3xl"}
        mx={"auto"}
        pl={8}
        flexDirection={"row"}
        alignItems={"center"}
      >
        <Box marginRight={5}>
          <Image src={shuffleIcon} boxSize={"70px"} />
        </Box>
        <Box>
          <ReactMarkDown components={ChakraComponents}>
            {content2}
          </ReactMarkDown>
        </Box>
      </Flex>

      <Flex
        maxW={"3xl"}
        mx={"auto"}
        pl={8}
        flexDirection={"row"}
        alignItems={"center"}
      >
        <Box marginRight={5}>
          <Image src={robotIcon} boxSize={"70px"} />
        </Box>
        <Box>
          <ReactMarkDown components={ChakraComponents}>
            {content3}
          </ReactMarkDown>
        </Box>
      </Flex>
    </div>
  );
};

export default HowToUsePage;
