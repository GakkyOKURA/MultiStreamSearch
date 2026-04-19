namespace MyApi.Models;

/// <summary>
/// 全件取得で出す
/// </summary>
public class VtuberResponse
{
    public List<VtuberDTO> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

/// <summary>
/// クライアントに返すのは id 以外 string のこの形
/// </summary>
public class VtuberDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string GroupName { get; set; } = "";
    public string PlatformName { get; set; } = "";
    public string ChannelId { get; set; } = "";

}

/// <summary>
///  取得して、 DataGrid の ComboBox カラムで出す
/// </summary>
public class GroupResponse
{
    public List<GroupTable> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class GroupTable
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

/// <summary>
/// 取得して、 DataGrid の ComboBox カラムで出す
/// </summary>
public class PlatformResponse
{
    public List<PlatformTable> Items { get; set; }= new();
    public int TotalCount { get; set; }
}

public class PlatformTable
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class VtuberTable
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int GroupId { get; set; }
    public int PlatformId { get; set; }
    public string ChannelId { get; set; } = "";
}
