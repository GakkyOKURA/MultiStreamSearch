import { Box } from "@chakra-ui/react";
import ReactMarkDown from "react-markdown";
import { PRIVACY_POLICY } from "../components/legalText/privasyPolicy";
import { ChakraComponents } from "../components/common/ChakraComponents";
import { CommonHeader } from "../components/common/commonHeader";

const PrivacyPolicyPage = () => {
  const content = PRIVACY_POLICY;
  return (
    <div>
      <CommonHeader />
      <Box maxW={"3xl"} mx={"auto"} p={8}>
        <ReactMarkDown components={ChakraComponents}>{content}</ReactMarkDown>
      </Box>
    </div>
  );
};

export default PrivacyPolicyPage;
