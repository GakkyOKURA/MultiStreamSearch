namespace MyApi.Models;

public static class IndividualChecker
{
    private static readonly List<string> CompanyNames = new List<string>
    {
            // --- 超大手・メジャー ---
        "にじさんじ", "NIJISANJI", "ANYCOLOR",
        "ホロライブ", "hololive", "カバー株式会社",
        "ぶいすぽ", "VSPO", "Brave group",
        "ななしいんく", "774inc", "774株式会社",
        "あおぎり高校", "viviON",
        
        // --- 大手・中堅・音楽系 ---
        "VEE", "Sony Music", "Verse n",
        "Neo-Porte", "ネオポルテ",
        "RIOT MUSIC", "KAMITSUBAKI STUDIO", "神椿スタジオ",
        "Re:AcT", "リアクト", "RK Music",
        "VShojo", ".LIVE", "どっとライブ", "アップランド",
        "GEMS COMPANY", "ディアステージ",
        "Palette Project", "パレプロ",
        "VOMS", "プロジェクトV",
        
        // --- 新興・勢いのある事務所 (2025-2026) ---
        "ミリプロ", "Million Production",
        "Varium", "バリアム",
        "のりプロ", "NORIPRO",
        "ハコネクト", "HACONECT",
        "すぺしゃりて", "Specialite",
        "にゃんたじあ", "サンリオ",
        "Vebop Project", "ビバッププロジェクト",
        "ほへとプロダクション", "Hoheto Production",
        "めたるびぃ", "FIRST STAGE PRODUCTION",
        
        // --- 配信ツール・プラットフォーム系 ---
        //"REALITY", "VASP", "IRIAM", "公式ライバー",
        
        // --- 法人・運営判定キーワード ---
        //"株式会社", "合同会社", "Inc.", "LLC", "Corp.", "Co., Ltd.",
        //"所属", "運営", "Official", "公式チャンネル",
        //"お問い合わせ先", "ファンレター送り先", "プレゼント窓口",
        //"Agency", "Production", "プロジェクト", "Project"
    };
    public static bool IsCompany(string descriptions)
    {
        return CompanyNames.Any(name => descriptions.Contains(name, StringComparison.OrdinalIgnoreCase));
    }
}
