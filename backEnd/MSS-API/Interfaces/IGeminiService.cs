using MyApi.Models;

namespace MyApi.Interfaces;

public interface IGeminiService
{
    Task<ChannelAnalysisResponse> SearchVtuberAnalysis();
    Task<ChannelAnalysisResponse> FetchVtuberAnalysis();
}
