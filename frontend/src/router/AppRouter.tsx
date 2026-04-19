import { Routes, Route, useLocation } from "react-router-dom";
import SearchPage from "../pages/SearchPage";
import VideoPage from "../pages/VideoPage";
import SearchSummaryPage from "../pages/AiSumaryPage";
import TermsOfServicePage from "../pages/TermsOfServicePage";
import PrivacyPolicyPage from "../pages/PrivacyPolicyPage";

export default function AppRouter() {
  const location = useLocation();
  return (
    <Routes location={location} key={location.pathname}>
      <Route path="/" element={<SearchPage />} />
      <Route path="/analysis" element={<SearchSummaryPage />} />
      <Route path="/video/:platform/:id" element={<VideoPage />} />
      <Route path="/termsOfService" element={<TermsOfServicePage />} />
      <Route path="/privacyPolicy" element={<PrivacyPolicyPage />} />
    </Routes>
  );
}
