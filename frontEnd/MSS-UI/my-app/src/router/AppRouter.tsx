import { Routes, Route } from "react-router-dom";
import SearchPage from "../pages/SearchPage";
import VideoPage from "../pages/VideoPage";
import SearchAnalysisPage from "../pages/AnalysisPage";

export default function AppRouter() {
  return (
    <Routes>
      <Route path="/" element={<SearchPage />} />
      <Route path="/analysis" element={<SearchAnalysisPage />} />
      <Route path="/video/:platform/:id" element={<VideoPage />} />
    </Routes>
  );
}
