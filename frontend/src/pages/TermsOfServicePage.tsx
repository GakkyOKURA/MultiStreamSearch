import { Box } from "@chakra-ui/react";
import ReactMarkDown from "react-markdown";
import { TERMS_OF_SERVICE } from "../components/legalText/termsOfService";
import { ChakraComponents } from "../components/common/ChakraComponents";
import { CommonHeader } from "../components/common/commonHeader";

const TermsOfServicePage = () => {
  const content = TERMS_OF_SERVICE;
  return (
    <div>
      <CommonHeader />
      <Box maxW={"3xl"} mx={"auto"} p={8}>
        <ReactMarkDown components={ChakraComponents}>{content}</ReactMarkDown>
      </Box>
    </div>
  );
};

export default TermsOfServicePage;
