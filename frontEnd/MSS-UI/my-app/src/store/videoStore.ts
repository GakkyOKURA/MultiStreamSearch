import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { UnifiedVideo } from "../components/unified/unifiedVideo";

type LiveStreamStore = {
  results: UnifiedVideo[];
  setResults: (videos: UnifiedVideo[]) => void;
};

export const useLiveStreamStore = create<LiveStreamStore>()(
  persist(
    (set) => ({
      results: [],
      setResults: (videos) => set({ results: videos }),
    }),
    {
      name: "liveSearchResults",
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

type ShortVideoStore = {
  results: UnifiedVideo[];
  setResults: (videos: UnifiedVideo[]) => void;
};

export const useShortVideoStore = create<ShortVideoStore>()(
  persist(
    (set) => ({
      results: [],
      setResults: (videos) => set({ results: videos }),
    }),
    {
      name: "shortSearchResults",
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
  current: UnifiedVideo | null;
  setCurrent: (video: UnifiedVideo | null) => void;
};

export const useCurrentVideoStore = create<CurrentVideoStore>()(
  persist(
    (set) => ({
      current: null,
      setCurrent: (video) => set({ current: video }),
    }),
    {
      name: "currentVideo",
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


