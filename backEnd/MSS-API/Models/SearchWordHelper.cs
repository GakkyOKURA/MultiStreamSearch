namespace MyApi.Models;

internal static class SearchWordHelper
{
    // 今後扱うゲームを増やす場合はここに追記していく
    public static readonly Dictionary<string, string> GameIds = new() 
    { 
        ["Dead by Daylight"] = "491487",
        //["VALORANT"] = "516575",
        //["Apex Legends"] = "511224" 
    };

    //// YouTube はゲーム名で検索を行う
    //internal static readonly string[] YouTubeSearchWords =
    //{
    //    "Dead by Daylight",
    //    "VALORANT"
    //};

    //// Twitch は gameId で検索を行う
    //internal static readonly string[] TwitchSearchWords =
    //{
    //    GameIds["Dead by Daylight"],
    //    GameIds["VALORANT"]
    //};
}
