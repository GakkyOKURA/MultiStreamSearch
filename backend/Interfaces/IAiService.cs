using MyApi.Models;

namespace MyApi.Interfaces;

public interface IAiService
{
    Task<ChannelSummaryResponse> FetchVtuberAnalysis();
}
