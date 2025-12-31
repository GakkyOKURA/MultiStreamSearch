import YouTubeCard from "../components/youtube/YouTubeCard";
import TwitchCard from "../components/twitch/TwitchCard";
import TwitchClipCard from "../components/twitch/TwitchClipCard";
import { UnifiedVideoCard } from "../components/unified/unifiedVideoCard";
import { useState, useEffect } from "react";
import { useDebounce } from "../hooks/useDebounce";
import { mapYouTubeToUnified } from "../components/unified/youtubeMapper";
import { mapTwitchStreamToUnified } from "../components/unified/twitchMapper";
import { mergeAlternating } from "../components/unified/unifiedVideo";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@chakra-ui/react";

export default function SearchPage() {
  const [query, setQuery] = useState("");
  const debouncedQuery = useDebounce(query, 300);
  const [cursor, setCursor] = useState<string | null>(null);
  const [clipCursor, setClipCursor] = useState<string | null>(null);
  const [ytCursor, setYtCursor] = useState<string | null>(null);
  const [ytsCursor, setYtsCursor] = useState<string | null>(null);
  const [categories, setCategories] = useState([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState<string | null>(
    null
  );
  const [activeTab, setActiveTab] = useState<"live" | "brief">("live");

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

  // // YouTube 検索
  // const searchYouTube = async () => {
  //   const res = await fetch(
  //     `https://localhost:7138/api/youtube/search?q=${query}`
  //   );
  //   const data = await res.json();
  //   setYtResults(data.items || []);
  //   console.log("Youtube検索クエリ:", query);
  // };

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

    console.log("YouTube検索:", query, "nextPageToken:", newCursor);
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
    console.log("Twitch検索クエリ:", selectedCategoryId, "cursor:", newCursor);
    // setTwitchResults(data.data || []); // Twitch は data.data に入ってる
    // console.log("Twitch検索クエリ:", selectedCategoryId);
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

    console.log("YouTube検索:", query, "nextPageToken:", newCursor);
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
    console.log("Twitch検索クエリ:", selectedCategoryId, "cursor:", newCursor);
    // setTwitchResults(data.data || []); // Twitch は data.data に入ってる
    // console.log("Twitch検索クエリ:", selectedCategoryId);
  };

  const searchLive = async (): Promise<void> => {
    await Promise.all([searchYouTube(), searchTwitch()]);
  };

  const mapAndMergeLive = async () => {
    await searchLive();

    const youtubeUnified = ytResults.map(mapYouTubeToUnified);
    const twitchUnified = twitchResults.map(mapTwitchStreamToUnified);

    const merged = mergeAlternating(youtubeUnified, twitchUnified);
    setUnifiedResults(merged);
  };

  // 🔥 インクリメンタルにカテゴリ検索
  useEffect(() => {
    if (!debouncedQuery) {
      setCategories([]);
      return;
    }

    const fetchCategories = async () => {
      const res = await fetch(
        `https://localhost:7138/api/twitch/categories?query=${debouncedQuery}`
      );
      const data = await res.json();
      setCategories(data.data || []);
    };

    fetchCategories();
  }, [debouncedQuery]);

  useEffect(() => {
    if (!selectedCategoryId) return;

    console.log("選択されたカテゴリID:", selectedCategoryId);

    // ここでカテゴリIDを使った処理を追加できる
    // 例: Twitch の配信一覧を取得する
    // fetch(`/api/twitch/streams?categoryId=${selectedCategoryId}`)
    //   .then(res => res.json())
    //   .then(data => console.log(data));

    mapAndMergeLive();
    // searchYouTube();
    // //searchYouTubeShort();
    // searchTwitch();
    // //searchTwitchClip();
    // console.log("検索したよ");

    // const youtubeUnified = ytResults.map(mapYouTubeToUnified);
    // const twitchUnified = twitchResults.map(mapTwitchStreamToUnified);

    // console.log(youtubeUnified.length);
    // console.log(twitchUnified.length);

    // const merged = mergeAlternating(youtubeUnified, twitchUnified);
    // setUnifiedResults(merged);
    // console.log(merged.length);
  }, [selectedCategoryId]);

  // return (
  //   <div style={{ maxWidth: "600px", margin: "0 auto", padding: "20px" }}>
  //     <h1>Twitch カテゴリ検索</h1>

  //     {/* 入力欄 */}
  //     <input
  //       value={query}
  //       onChange={(e) => setQuery(e.target.value)}
  //       placeholder="ゲーム名を入力"
  //       style={{
  //         width: "100%",
  //         padding: "10px",
  //         fontSize: "16px",
  //         borderRadius: "8px",
  //         border: "1px solid #ccc",
  //       }}
  //     />

  //     {/* 🔽 ドロップダウン表示 */}
  //     {categories.length > 0 && (
  //       <div
  //         style={{
  //           marginTop: "8px",
  //           border: "1px solid #ddd",
  //           borderRadius: "8px",
  //           background: "white",
  //           boxShadow: "0 4px 12px rgba(0,0,0,0.1)",
  //         }}
  //       >
  //         {categories.map((cat: any) => (
  //           <div
  //             key={cat.id}
  //             style={{
  //               padding: "10px",
  //               borderBottom: "1px solid #eee",
  //               cursor: "pointer",
  //             }}
  //             onClick={() => {
  //               console.log("選択されたカテゴリID:", cat.id);
  //               console.log("カテゴリ名:", cat.name);

  //               // 入力欄にカテゴリ名を反映
  //               setQuery(cat.name);

  //               // ドロップダウンを閉じる
  //               setCategories([]);

  //               // ★ 必要ならカテゴリIDを state に保存
  //               setSelectedCategoryId(cat.id);

  //               //searchYouTube();
  //               // searchTwitch();
  //             }}
  //           >
  //             {cat.name}
  //           </div>
  //         ))}
  //       </div>
  //     )}

  //     {/* <Tabs defaultValue="live"> */}
  //     {/* <TabsList>
  //       <TabsTrigger value="live">Live</TabsTrigger>
  //       <TabsTrigger value="brief">Brief</TabsTrigger>
  //     </TabsList>

  //     <TabsContent value="live">
  //       {unifiedResult
  //         .filter((item) => item.type === "live" || item.type === "Video")
  //         .map((item) => (
  //           <UnifiedVideoCard key={item.id} item={item} />
  //         ))}
  //     </TabsContent>

  //     <TabsContent value="brief">
  //       {unifiedResult
  //         .filter((item) => item.type === "short" || item.type === "clip")
  //         .map((item) => (
  //           <UnifiedVideoCard key={item.id} item={item} />
  //         ))}
  //     </TabsContent> */}
  //     {/* </Tabs> */}

  //     {/* Unified */}
  //     <h2>Results</h2>

  //     {unifiedResult.map((item) => (
  //       <UnifiedVideoCard key={item.id} item={item} />
  //     ))}

  //     {/* YouTube */}
  //     {/* <h2>YouTube</h2>
  //     {ytResults.map((item: any) => (
  //       <YouTubeCard key={item.id.videoId} item={item} />
  //     ))} */}

  //     <button onClick={() => searchYouTube(true)} disabled={!ytCursor}>
  //       もっと見る
  //     </button>

  //     {/* Twitch */}
  //     {/* <h2 style={{ marginTop: "40px" }}>Twitch</h2>
  //     {twitchResults.map((item: any) => (
  //       <TwitchCard key={item.id} item={item} />
  //     ))} */}

  //     <button onClick={() => searchTwitch(true)} disabled={!cursor}>
  //       もっと見る
  //     </button>

  //     {/* YouTubeShort */}
  //     <h2>YouTubeShort</h2>
  //     {ytsResults.map((item: any) => (
  //       <YouTubeCard key={item.id.videoId} item={item} />
  //     ))}

  //     <button onClick={() => searchYouTubeShort(true)} disabled={!ytsCursor}>
  //       もっと見る
  //     </button>

  //     {/* TwitchClip */}
  //     <h2 style={{ marginTop: "40px" }}>TwitchClip</h2>
  //     {twitchClipResults.map((item: any) => (
  //       <TwitchClipCard key={item.id} item={item} />
  //     ))}

  //     <button onClick={() => searchTwitchClip(true)} disabled={!clipCursor}>
  //       もっと見る
  //     </button>
  //   </div>
  // );

  return (
    <div style={{ maxWidth: "600px", margin: "0 auto", padding: "20px" }}>
      <h1>Twitch カテゴリ検索</h1>

      {/* 入力欄 */}
      <input
        value={query}
        onChange={(e) => setQuery(e.target.value)}
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
                setQuery(cat.name);
                setCategories([]);
                setSelectedCategoryId(cat.id);
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
        // onValueChange={(details) => {
        //   if (details.value === "live") {
        //     searchYouTubeLive();
        //     searchTwitchLive();
        //   }
        //   if (details.value === "brief") {
        //     searchYouTubeShorts();
        //     searchTwitchClips();
        //   }
        // }}
      >
        <TabsList>
          <TabsTrigger value="live">Live</TabsTrigger>
          <TabsTrigger value="brief">Brief</TabsTrigger>
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

        {/* Brief */}
        <TabsContent value="brief">
          {unifiedResult
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
