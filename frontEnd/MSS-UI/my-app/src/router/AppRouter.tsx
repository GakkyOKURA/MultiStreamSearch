import { Routes, Route } from "react-router-dom";
import SearchPage from "../pages/SearchPage";
import VideoPage from "../pages/VideoPage";

export default function AppRouter() {
  return (
    <Routes>
      <Route path="/" element={<SearchPage />} />
      <Route path="/video/:platform/:id" element={<VideoPage />} />
    </Routes>
  );
}
