using System.Data;
using Microsoft.Data.SqlClient;

public class ChartOfAccountsRepository
{
    private readonly string _connectionString;

    public ChartOfAccountsRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public void ManageAccount(string mode, ChartOfAccount acc)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@Mode", mode);
        cmd.Parameters.AddWithValue("@Id", acc.Id);
        cmd.Parameters.AddWithValue("@AccountName", acc.AccountName ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ParentId", acc.ParentId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@AccountCode", acc.AccountCode ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@AccountType", acc.AccountType ?? (object)DBNull.Value);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public List<ChartOfAccount> GetAll()
    {
        var list = new List<ChartOfAccount>();
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand("SELECT * FROM ChartOfAccounts", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ChartOfAccount
            {
                Id = (int)reader["Id"],
                AccountName = reader["AccountName"].ToString(),
                ParentId = reader["ParentId"] as int?,
                AccountCode = reader["AccountCode"].ToString(),
                AccountType = reader["AccountType"].ToString()
            });
        }

        return list;
    }

    public ChartOfAccount GetById(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        using var cmd = new SqlCommand("SELECT * FROM ChartOfAccounts WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new ChartOfAccount
            {
                Id = (int)reader["Id"],
                AccountName = reader["AccountName"].ToString(),
                ParentId = reader["ParentId"] as int?,
                AccountCode = reader["AccountCode"].ToString(),
                AccountType = reader["AccountType"].ToString()
            };
        }

        return null;
    }
}
