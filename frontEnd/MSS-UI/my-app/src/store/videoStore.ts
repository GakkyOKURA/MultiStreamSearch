import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { VideoDataDTO } from "../components/videos/videoData";
import type { VideoWithAnalysisDTO } from "../components/videos/videoWithAnalysis";

type VideoDataStore = {
  results: VideoDataDTO[];
  setResults: (videos: VideoDataDTO[]) => void;
};

export const useVideoDataStore = create<VideoDataStore>()(
  persist(
    (set) => ({
      results: [],
      setResults: (videos) => set({ results: videos }),
    }),
    {
      name: "videoDataResults",
      storage: {
        getItem: (name) => {
          const value = sessionStorage.getItem(name);
          return value ? JSON.parse(value) : null;
        },
        setItem: (name, value) => {
          sessionStorage.setItem(name, JSON.stringify(value));
        },
        removeItem: (name) => {
          sessionStorage.removeItem(name);
        },
      },
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
      storage: {
        getItem: (name) => {
          const value = sessionStorage.getItem(name);
          return value ? JSON.parse(value) : null;
        },
        setItem: (name, value) => {
          sessionStorage.setItem(name, JSON.stringify(value));
        },
        removeItem: (name) => {
          sessionStorage.removeItem(name);
        },
      },
    }
  )
);

type VideoWithAnalysisStore = {
  results: VideoWithAnalysisDTO[];
  setResults: (videos: VideoWithAnalysisDTO[]) => void;
};

export const useVideoWithAnalysisStore = create<VideoWithAnalysisStore>()(
  persist(
    (set) => ({
      results: [],
      setResults: (videos) => set({ results: videos }),
    }),
    {
      name: "videoWithAnalysisResults",
      storage: {
        getItem: (name) => {
          const value = sessionStorage.getItem(name);
          return value ? JSON.parse(value) : null;
        },
        setItem: (name, value) => {
          sessionStorage.setItem(name, JSON.stringify(value));
        },
        removeItem: (name) => {
          sessionStorage.removeItem(name);
        },
      },
    }
  )
);

type CurrentVideoWithAnalysisStore = {
  current: VideoWithAnalysisDTO | null;
  setCurrent: (video: VideoWithAnalysisDTO | null) => void;
};

export const useCurrentVideoWithAnalysisStore = create<CurrentVideoWithAnalysisStore>()(
  persist(
    (set) => ({
      current: null,
      setCurrent: (video) => set({ current: video }),
    }),
    {
      name: "currentVideoWithAnalysis",
      storage: {
        getItem: (name) => {
          const value = sessionStorage.getItem(name);
          return value ? JSON.parse(value) : null;
        },
        setItem: (name, value) => {
          sessionStorage.setItem(name, JSON.stringify(value));
        },
        removeItem: (name) => {
          sessionStorage.removeItem(name);
        },
      },
    }
  )
);

