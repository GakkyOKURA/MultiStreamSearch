using MyApi.Models;

namespace MyApi.Interfaces;

public interface IAiService
{
    Task<ChannelSummaryResponse> SearchVtuberAnalysis();
    Task<ChannelSummaryResponse> FetchVtuberAnalysis();
}
