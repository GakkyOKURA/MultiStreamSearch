import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";
import type { VideoDataDTO } from "../components/videos/videoData";
import type { VideoWithSummaryDTO } from "../components/videos/videoWithSummary";

type VideoDataStore = {
  results: VideoDataDTO[];
  setResults: (videos: VideoDataDTO[]) => void;
};

// バックエンドから取得した動画リストを扱う
export const useVideoDataStore = create<VideoDataStore>()(
  persist(
    (set) => ({
      results: [],
      setResults: (videos) => set({ results: videos }),
    }),
    {
      name: "videoDataResults",
      storage: createJSONStorage(() => sessionStorage),
    }
  )
);

type CurrentVideoStore = {
  current: VideoDataDTO | null;
  setCurrent: (video: VideoDataDTO | null) => void;
};

export const useCurrentVideoDataStore = create<CurrentVideoStore>()(
  persist(
    (set) => ({
      current: null,
      setCurrent: (video) => set({ current: video }),
    }),
    {
      name: "currentVideoData",
      storage: createJSONStorage(() => sessionStorage),
    }
  )
);

type VideoWithAnalysisStore = {
  results: VideoWithSummaryDTO[];
  setResults: (videos: VideoWithSummaryDTO[]) => void;
};

export const useVideoWithAnalysisStore = create<VideoWithAnalysisStore>()(
  persist(
    (set) => ({
      results: [],
      setResults: (videos) => set({ results: videos }),
    }),
    {
      name: "videoWithAnalysisResults",
      storage: createJSONStorage(() => sessionStorage),
    }
  )
);

type CurrentVideoWithAnalysisStore = {
  current: VideoWithSummaryDTO | null;
  setCurrent: (video: VideoWithSummaryDTO | null) => void;
};

export const useCurrentVideoWithAnalysisStore = create<CurrentVideoWithAnalysisStore>()(
  persist(
    (set) => ({
      current: null,
      setCurrent: (video) => set({ current: video }),
    }),
    {
      name: "currentVideoWithAnalysis",
      storage: createJSONStorage(() => sessionStorage),
    }
  )
);

type NeedReloadStore = {
  isReloadNeeded: boolean;
  setIsReloadNeeded: (isNeeded: boolean) => void;
};

export const useNeedReloadStore = create<NeedReloadStore>()(
  persist(
    (set) => ({
      isReloadNeeded: true, // ストレージに何もない時はこれが使われる
      setIsReloadNeeded: (needed) => set({ isReloadNeeded:needed }),
    }),
    {
      name: "need-reload-storage",
      // これだけで getItem/setItem/removeItem を安全に実装してくれます
      storage: createJSONStorage(() => sessionStorage),
    }
  )
);

