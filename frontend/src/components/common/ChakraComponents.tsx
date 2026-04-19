import { Heading, Text, Link, List, Separator, Box } from "@chakra-ui/react";

// 1. v3 形式のレンダラー設定
export const ChakraComponents = {
  // ## 見出し
  h2: (props: any) => (
    <Heading
      as="h2"
      size="xl"
      mt="8"
      mb="4"
      borderBottom="1px solid"
      borderColor="border.subtle"
      pb="2"
      {...props}
    />
  ),
  // ### 見出し
  h3: (props: any) => <Heading as="h3" size="md" mt="6" mb="2" {...props} />,
  // 通常の段落
  p: (props: any) => <Text mb="4" lineHeight="relaxed" {...props} />,

  // 箇条書きの親 (ul)
  ul: (props: any) => (
    <List.Root mb="4" ml="6" gap="2" as="ul">
      {props.children}
    </List.Root>
  ),

  // 箇条書きの子 (li)
  // List.Item を使うと ContextError が出るため、chakra.li でスタイルだけ当てる
  // chakra.li の代わりに Box を li として使う
  li: (props: any) => <Box as="li" display="list-item" {...props} />,

  // 番号付きリスト (ol) も念のため
  ol: (props: any) => (
    <List.Root mb="4" ml="6" gap="2" as="ol" variant="plain">
      {props.children}
    </List.Root>
  ),

  // リンク
  a: ({ href, children }: any) => (
    <Link
      href={href}
      target="_blank"
      rel="noopener noreferrer"
      color="blue.600"
      fontWeight="medium"
      display="inline-flex"
      alignItems="center"
      gap="1"
      textDecoration="underline" // リンクだと分かりやすく
    >
      {children} <span>↗</span>
    </Link>
  ),
  // --- (水平線)
  hr: () => <Separator my="10" />,
};
