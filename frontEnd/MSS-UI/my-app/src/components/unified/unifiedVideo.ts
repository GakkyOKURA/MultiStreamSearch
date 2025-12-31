export interface UnifiedVideo {
  id: string;                 // 動画ID（YouTube or Twitch）
  title: string;              // タイトル
  thumbnailUrl: string;       // サムネイルURL
  url: string;                // 実際に開くURL
  source: "youtube" | "twitch"; // どっちのサービスか
  type: "twitchLive" | "youtubeLive" | "short" | "clip"; // 種類
  channelName: string;        // 配信者 or チャンネル名
  viewerCount?: number;       // ライブの場合のみ
  publishedAt?: string;       // 投稿日
}

export function mergeAlternating<T>(a: T[], b: T[]): T[] {
  const result: T[] = [];
  const max = Math.max(a.length, b.length);

  for (let i = 0; i < max; i++) {
    if (i < a.length) result.push(a[i]);
    if (i < b.length) result.push(b[i]);
  }

  return result;
}
