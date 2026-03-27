using MyApi.Models;
using Npgsql;

namespace MyApi.Services;

public class VtuberRepository
{
    private const string UniqueViolationErrorCode = "23505";
    private const string ForeignKeyViolationErrorCode = "23503";
    private readonly string _connectionString;

    public VtuberRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// アプリ起動時にテーブルを作成する
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        using var conn = new NpgsqlConnection(_connectionString);
        // 非同期で接続
        await conn.OpenAsync();

        var sql = @"
            CREATE TABLE IF NOT EXISTS groups (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS platforms (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS vtubers (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                group_id INTEGER REFERENCES groups(id),
                platform_id INTEGER REFERENCES platforms(id),
                channel_id TEXT NOT NULL,
                UNIQUE (platform_id, channel_id)
            );";

        using var cmd = new NpgsqlCommand(sql, conn);
        // 非同期で実行
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<VtuberResponse> GetAllVtubersAsync()
    {
        var list = new List<VtuberDTO>();

        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = @"
            SELECT v.id, v.name, g.name, p.name, v.channel_id
            FROM vtubers v
            JOIN groups g ON v.group_id = g.id
            JOIN platforms p ON v.platform_id = p.id";

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var dto = new VtuberDTO
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                GroupName = reader.GetString(2),
                PlatformName = reader.GetString(3),
                ChannelId = reader.GetString(4)
            };
            list.Add(dto);
        }

        return new VtuberResponse { Items = list, TotalCount = list.Count };
    }

    public async Task<VtuberResponse> GetVtubersByNameAsync(string searchName)
    {
        var items = new List<VtuberDTO>();

        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = @"
            SELECT 
                v.id, 
                v.name, 
                g.name AS group_name, 
                p.name AS platform_name, 
                v.channel_id
            FROM vtubers v
            JOIN groups g ON v.group_id = g.id
            JOIN platforms p ON v.platform_id = p.id
            WHERE v.name LIKE @searchName || '%';";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("searchName", searchName);

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new VtuberDTO
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                GroupName = reader.GetString(2),
                PlatformName = reader.GetString(3),
                ChannelId = reader.GetString(4)
            });
        }

        return new VtuberResponse
        {
            Items = items,
            TotalCount = items.Count
        };
    }

    public async Task<VtuberResponse> GetVtubersByGroupAsync(string groupName)
    {
        var response = new VtuberResponse();
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = @"
            SELECT v.id, v.name, g.name as group_name, p.name as platform_name, v.channel_id
            FROM vtubers v
            JOIN groups g ON v.group_id = g.id
            JOIN platforms p ON v.platform_id = p.id
            WHERE g.name = @groupName;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("groupName", groupName);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            response.Items.Add(new VtuberDTO
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                GroupName = reader.GetString(2),
                PlatformName = reader.GetString(3),
                ChannelId = reader.GetString(4)
            });
        }

        return response;
    }

    public async Task<VtuberResponse> GetVtubersByNmeOrGroupAsync(string searchName, string groupName)
    {
        var items = new List<VtuberDTO>();

        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // 共通のベースSQL
        var sql = @"
        SELECT v.id, v.name, g.name AS group_name, p.name AS platform_name, v.channel_id
        FROM vtubers v
        JOIN groups g ON v.group_id = g.id
        JOIN platforms p ON v.platform_id = p.id
        WHERE 1=1"; // 1=1 は動的条件を追加しやすくするための定石
        // 追加する条件はすべて AND から始めて OK になる

        // 条件の追加
        if (!string.IsNullOrEmpty(searchName))
        {
            sql += " AND v.name LIKE @searchName || '%'";
        }
        if (!string.IsNullOrEmpty(groupName))
        {
            sql += " AND g.name = @groupName";
        }

        using var cmd = new NpgsqlCommand(sql, conn);

        // パラメータのセット
        if (!string.IsNullOrEmpty(searchName))
        {
            cmd.Parameters.AddWithValue("searchName", searchName);
        }
        if (!string.IsNullOrEmpty(groupName))
        {
            cmd.Parameters.AddWithValue("groupName", groupName);
        }

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new VtuberDTO
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                GroupName = reader.GetString(2),
                PlatformName = reader.GetString(3),
                ChannelId = reader.GetString(4)
            });
        }

        return new VtuberResponse
        {
            Items = items,
            TotalCount = items.Count
        };
    }

    public async Task<int> AddVtuberAsync(string vtuberName, string groupName, string platformName, string channelId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = @"
        INSERT INTO vtubers (name, group_id, platform_id, channel_id)
        VALUES (
            @name, 
            (SELECT id FROM groups WHERE name = @groupName), 
            (SELECT id FROM platforms WHERE name = @platformName), 
            @channelId
        )
        RETURNING id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", vtuberName);
        cmd.Parameters.AddWithValue("groupName", groupName);
        cmd.Parameters.AddWithValue("platformName", platformName);
        cmd.Parameters.AddWithValue("channelId", channelId);

        try
        {
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (PostgresException ex) when (ex.SqlState == UniqueViolationErrorCode)
        {
            throw new Exception($"Vtuber '{vtuberName}' は既に登録されています。");
        }
    }

    public async Task UpdateVtuberAsync(int id, string newName, string newGroupName, string newPlatformName, string newChannelId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = @"
            UPDATE vtubers
            SET 
                name = @newName,
                group_id = (SELECT id FROM groups WHERE name = @groupName),
                platform_id = (SELECT id FROM platforms WHERE name = @platformName),
                channel_id = @channelId
            WHERE id = @id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("newName", newName);
        cmd.Parameters.AddWithValue("groupName", newGroupName);
        cmd.Parameters.AddWithValue("platformName", newPlatformName);
        cmd.Parameters.AddWithValue("channelId", newChannelId);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
        {
            throw new Exception($"ID: {id} の VTuber が見つからなかったため、更新できませんでした。");
        }
    }

    public async Task DeleteVtuberAsync(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "DELETE FROM vtubers WHERE id = @id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
        {
            throw new Exception($"ID: {id} の VTuber は見つかりませんでした。");
        }
    }

    public async Task<GroupResponse> GetAllGroupsAsync()
    {
        var response = new GroupResponse();
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "SELECT id, name FROM groups ORDER BY id ASC;";
        using var cmd = new NpgsqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            response.Items.Add(new GroupTable
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }

        return response;
    }

    public async Task<int> AddGroupAsync(string groupName)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "INSERT INTO groups (name) VALUES (@name) RETURNING id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", groupName);

        try
        {
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (PostgresException ex) when (ex.SqlState == UniqueViolationErrorCode)
        {
            throw new Exception($"グループ '{groupName}' は既に登録されています。");
        }
    }

    public async Task UpdateGroupAsync(int id, string newName)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "UPDATE groups SET name = @name WHERE id = @id;";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", newName);
        cmd.Parameters.AddWithValue("id", id);

        if (await cmd.ExecuteNonQueryAsync() == 0)
        {
            throw new Exception($"ID: {id} のグループは見つかりませんでした。");
        }
    }

    public async Task DeleteGroupAsync(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        try
        {
            var sql = "DELETE FROM groups WHERE id = @id;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            if (await cmd.ExecuteNonQueryAsync() == 0)
            {
                throw new Exception($"ID: {id} のグループは見つかりませんでした。");
            }
        }
        catch (PostgresException ex) when (ex.SqlState == ForeignKeyViolationErrorCode)
        {
            throw new Exception("このグループに所属している VTuber が存在するため、削除できません。");
        }
    }

    public async Task<PlatformResponse> GetAllPlatformsAsync()
    {
        var response = new PlatformResponse();
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "SELECT id, name FROM platforms ORDER BY id ASC;";
        using var cmd = new NpgsqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            response.Items.Add(new PlatformTable
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }

        return response;
    }

    public async Task<int> AddPlatformAsync(string platformName)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "INSERT INTO platforms (name) VALUES (@name) RETURNING id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", platformName);

        try
        {
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (PostgresException ex) when (ex.SqlState == UniqueViolationErrorCode)
        {
            throw new Exception($"プラットフォーム '{platformName}' は既に登録されています。");
        }
    }

    public async Task UpdatePlatformAsync(int id, string newName)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "UPDATE platforms SET name = @name WHERE id = @id;";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", newName);
        cmd.Parameters.AddWithValue("id", id);

        if (await cmd.ExecuteNonQueryAsync() == 0)
        {
            throw new Exception($"ID: {id} のプラットフォームは見つかりませんでした。");
        }
    }

    public async Task DeletePlatformAsync(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "DELETE FROM platforms WHERE id = @id;";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        try
        {
            if (await cmd.ExecuteNonQueryAsync() == 0)
            {
                throw new Exception($"ID: {id} のプラットフォームは見つかりませんでした。");
            }
        }
        catch (PostgresException ex) when (ex.SqlState == ForeignKeyViolationErrorCode)
        {
            throw new Exception("このプラットフォームを利用している VTuber のデータが存在するため、削除できません。");
        }
    }
}
