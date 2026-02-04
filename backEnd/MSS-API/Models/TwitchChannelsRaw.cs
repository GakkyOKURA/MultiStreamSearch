using System.Text.Json.Serialization;

namespace MyApi.Models;

public class TwitchUserResponse
{
    [JsonPropertyName("data")]
    public List<TwitchUserRaw> Data { get; set; } = new();
}

public class TwitchUserRaw
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("login")]
    public string Login { get; set; } = ""; // ユーザー名の英語・ID用（URL等に使う）

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = ""; // 表示用の名前（日本語など）

    [JsonPropertyName("type")]
    public string Type { get; set; } = ""; // 運営権限など（通常は空）

    [JsonPropertyName("broadcaster_type")]
    public string BroadcasterType { get; set; } = ""; // "partner", "affiliate", or ""

    [JsonPropertyName("description")]
    public string Description { get; set; } = ""; // 自己紹介文（最重要！）

    [JsonPropertyName("profile_image_url")]
    public string ProfileImageUrl { get; set; } = "";

    [JsonPropertyName("offline_image_url")]
    public string OfflineImageUrl { get; set; } = "";

    [JsonPropertyName("view_count")]
    public int ViewCount { get; set; } // チャンネルの通算視聴回数（人気度の指標）

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } // アカウント作成日（古参かどうかの判定に）
}