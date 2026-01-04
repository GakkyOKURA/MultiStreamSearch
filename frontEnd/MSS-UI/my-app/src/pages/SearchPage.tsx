import { UnifiedVideoCard } from "../components/unified/unifiedVideoCard";
import { useState, useEffect } from "react";
import { useDebounce } from "../hooks/useDebounce";
import {
  mapShortToUnified,
  mapYouTubeToUnified,
} from "../components/unified/youtubeMapper";
import {
  mapTwitchClipToUnified,
  mapTwitchStreamToUnified,
} from "../components/unified/twitchMapper";
import { mergeAlternating } from "../components/unified/unifiedVideo";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@chakra-ui/react";

export default function SearchPage() {
  const [query, setQuery] = useState("");
  const [skipDebounce, setSkipDebounce] = useState(false);
  const debouncedQuery = useDebounce(query, 300, skipDebounce);
  const [cursor, setCursor] = useState<string | null>(null);
  const [clipCursor, setClipCursor] = useState<string | null>(null);
  const [ytCursor, setYtCursor] = useState<string | null>(null);
  const [ytsCursor, setYtsCursor] = useState<string | null>(null);
  const [categories, setCategories] = useState([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState<string | null>(
    null
  );
  const [currentTab, setCurrentTab] = useState("live");
  const [isLiveSearchDone, setLiveSearchDone] = useState(false);
  const [isSnapSearchDone, setSnapSearchDone] = useState(false);

  // YouTube
  const [ytResults, setYtResults] = useState<any[]>([]);

  // YouTubeShort
  const [ytsResults, setYtsResults] = useState<any[]>([]);

  // Twitch
  const [twitchResults, setTwitchResults] = useState<any[]>([]);

  // TwitchClip
  const [twitchClipResults, setTwitchClipResults] = useState<any[]>([]);

  // UnifiedVideo
  const [unifiedResult, setUnifiedResults] = useState<any[]>([]);

  // UnifiedVideo(snap)
  const [snapUnifiedResult, setSnapUnifiedResults] = useState<any[]>([]);

  // YouTube 検索
  const searchYouTube = async (isLoadMore = false) => {
    const url = isLoadMore
      ? `https://localhost:7138/api/youtube/search?q=${query}&pageToken=${ytCursor}`
      : `https://localhost:7138/api/youtube/search?q=${query}`;

    const res = await fetch(url);
    const data = await res.json();

    // YouTube は nextPageToken を使う
    const newCursor = data.nextPageToken || null;

    if (isLoadMore) {
      // 追加読み込み
      setYtResults((prev) => [...prev, ...data.items]);
    } else {
      // 最初の検索
      setYtResults(data.items);
    }

    setYtCursor(newCursor);

    // ★ 最新の値を返す（これが重要）
    // ytResults は次のレンダリングで更新
    return {
      items: data.items,
      nextCursor: newCursor,
    };
  };

  // Twitch 検索
  const searchTwitch = async (isLoadMore = false) => {
    if (!selectedCategoryId) {
      return;
    }

    const url = isLoadMore
      ? `https://localhost:7138/api/twitch/streams?categoryId=${selectedCategoryId}&cursor=${cursor}`
      : `https://localhost:7138/api/twitch/streams?categoryId=${selectedCategoryId}`;

    const res = await fetch(url);
    const data = await res.json();

    // data.data = 配信データ
    // data.pagination.cursor = 次のページのカーソル
    const newCursor = data.pagination?.cursor || null;

    if (isLoadMore) {
      // 追加読み込み
      setTwitchResults((prev) => [...prev, ...data.data]);
    } else {
      // 最初の検索
      setTwitchResults(data.data);
    }

    setCursor(newCursor);
    // ★ 最新の値を返す（これが重要）
    // twitchResults は次のレンダリングで更新
    return {
      items: data.data,
      nextCursor: newCursor,
    };
  };

  // YouTubeShort 検索
  const searchYouTubeShort = async (isLoadMoreShort = false) => {
    const url = isLoadMoreShort
      ? `https://localhost:7138/api/youtube/short?q=${query}&period=Month&pageToken=${ytsCursor}`
      : `https://localhost:7138/api/youtube/short?q=${query}&period=Month`;

    const res = await fetch(url);
    const data = await res.json();

    // YouTube は nextPageToken を使う
    const newCursor = data.nextPageToken || null;

    if (isLoadMoreShort) {
      // 追加読み込み
      setYtsResults((prev) => [...prev, ...data.items]);
    } else {
      // 最初の検索
      setYtsResults(data.items);
    }

    setYtsCursor(newCursor);

    return {
      items: data.items,
      nextCursor: newCursor,
    };
  };

  // TwitchClip 検索
  const searchTwitchClip = async (isLoadMoreClip = false) => {
    if (!selectedCategoryId) {
      return;
    }

    const url = isLoadMoreClip
      ? `https://localhost:7138/api/twitch/clips?categoryId=${selectedCategoryId}&period=Month&cursor=${clipCursor}`
      : `https://localhost:7138/api/twitch/clips?categoryId=${selectedCategoryId}&period=Month`;

    const res = await fetch(url);
    const data = await res.json();

    // data.data = 配信データ
    // data.pagination.cursor = 次のページのカーソル
    const newCursor = data.pagination?.cursor || null;

    if (isLoadMoreClip) {
      // 追加読み込み
      setTwitchClipResults((prev) => [...prev, ...data.data]);
    } else {
      // 最初の検索
      setTwitchClipResults(data.data);
    }

    setClipCursor(newCursor);
    //console.log("Twitch検索クエリ:", selectedCategoryId, "cursor:", newCursor);
    // setTwitchResults(data.data || []); // Twitch は data.data に入ってる
    return {
      items: data.data,
      nextCursor: newCursor,
    };
  };

  const searchLive = async (): Promise<void> => {
    const [youtube, twitch] = await Promise.all([
      searchYouTube(),
      searchTwitch(),
    ]);
    mapAndMergeLive([youtube, twitch]);
  };

  const searchSnap = async (): Promise<void> => {
    const [short, clip] = await Promise.all([
      searchYouTubeShort(),
      searchTwitchClip(),
    ]);
    mapAndMergeSnap([short, clip]);
  };

  const mapAndMergeLive = ([youtube, twitch]: [any, any]) => {
    const youtubeUnified = youtube.items.map(mapYouTubeToUnified);
    const twitchUnified = twitch.items.map(mapTwitchStreamToUnified);

    const merged = mergeAlternating(youtubeUnified, twitchUnified);
    setUnifiedResults(merged);
    setLiveSearchDone(true);
  };

  const mapAndMergeSnap = async ([short, clip]: [any, any]) => {
    const shortUnified = short.items.map(mapShortToUnified);
    const clipUnified = clip.items.map(mapTwitchClipToUnified);

    const merged = mergeAlternating(shortUnified, clipUnified);
    setSnapUnifiedResults(merged);
    setSnapSearchDone(true);
  };

  // 🔥 インクリメンタルにカテゴリ検索
  useEffect(() => {
    if (!debouncedQuery) {
      setCategories([]);
      return;
    }
    fetchCategories();
  }, [debouncedQuery]);

  const fetchCategories = async () => {
    const res = await fetch(
      `https://localhost:7138/api/twitch/categories?query=${debouncedQuery}`
    );
    const data = await res.json();
    setCategories(data.data || []);
  };

  useEffect(() => {
    if (!selectedCategoryId) {
      return;
    }

    setLiveSearchDone(false);
    setSnapSearchDone(false);
    console.log("categories are selected  :" + selectedCategoryId);

    if (currentTab === "live") {
      searchLive();
    } else {
      searchSnap();
    }
  }, [selectedCategoryId]);

  return (
    <div style={{ maxWidth: "600px", margin: "0 auto", padding: "20px" }}>
      <h1>Twitch カテゴリ検索</h1>

      {/* 入力欄 */}
      <input
        value={query}
        onChange={(e) => {
          setSkipDebounce(false);
          setQuery(e.target.value);
        }}
        placeholder="ゲーム名を入力"
        style={{
          width: "100%",
          padding: "10px",
          fontSize: "16px",
          borderRadius: "8px",
          border: "1px solid #ccc",
        }}
      />

      {/* 🔽 ドロップダウン表示 */}
      {categories.length > 0 && (
        <div
          style={{
            marginTop: "8px",
            border: "1px solid #ddd",
            borderRadius: "8px",
            background: "white",
            boxShadow: "0 4px 12px rgba(0,0,0,0.1)",
          }}
        >
          {categories.map((cat: any) => (
            <div
              key={cat.id}
              style={{
                padding: "10px",
                borderBottom: "1px solid #eee",
                cursor: "pointer",
              }}
              onClick={() => {
                setSkipDebounce(true);
                setQuery(cat.name);
                setSelectedCategoryId(cat.id);
                setCategories([]);
              }}
            >
              {cat.name}
            </div>
          ))}
        </div>
      )}

      {/* Unified */}
      <h2>Results</h2>

      {/* ▼▼▼ ここから Tabs ▼▼▼ */}
      <Tabs.Root
        defaultValue="live"
        onValueChange={(details) => {
          setCurrentTab(details.value);

          if (!selectedCategoryId) {
            return;
          }

          if (details.value === "live") {
            if (isLiveSearchDone) {
              return;
            }
            searchLive();
          } else {
            if (isSnapSearchDone) {
              return;
            }
            searchSnap();
          }
        }}
      >
        <TabsList>
          <TabsTrigger value="live">Live</TabsTrigger>
          <TabsTrigger value="snap">Snap</TabsTrigger>
        </TabsList>

        {/* Live */}
        <TabsContent value="live">
          {unifiedResult
            .filter(
              (item) =>
                item.type === "twitchLive" || item.type === "youtubeLive"
            )
            .map((item) => (
              <UnifiedVideoCard key={item.id} item={item} />
            ))}
        </TabsContent>

        {/* Snap */}
        <TabsContent value="snap">
          {snapUnifiedResult
            .filter((item) => item.type === "short" || item.type === "clip")
            .map((item) => (
              <UnifiedVideoCard key={item.id} item={item} />
            ))}
        </TabsContent>
      </Tabs.Root>
      {/* ▲▲▲ Tabs 終わり ▲▲▲ */}
    </div>
  );
}
