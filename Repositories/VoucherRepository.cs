using AccountManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

[Authorize(Roles = "Admin,Accountant")]

public class VoucherRepository
{
    private readonly string _connectionString;

    public VoucherRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public void SaveVoucher(VoucherViewModel model, string createdBy)
    {
        using var con = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_SaveVoucher", con)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@VoucherType", model.VoucherType ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ReferenceNo", model.ReferenceNo ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@VoucherDate", model.VoucherDate);
        cmd.Parameters.AddWithValue("@CreatedBy", createdBy ?? (object)DBNull.Value);

        var dt = new DataTable();
        dt.Columns.Add("AccountId", typeof(int));
        dt.Columns.Add("DebitAmount", typeof(decimal));
        dt.Columns.Add("CreditAmount", typeof(decimal));

        foreach (var line in model.Entries)
        {
            dt.Rows.Add(line.AccountId, line.DebitAmount, line.CreditAmount);
        }

        var detailsParam = new SqlParameter("@VoucherDetailsTbl", SqlDbType.Structured)
        {
            Value = dt,
            TypeName = "VoucherDetailsType"
        };
        cmd.Parameters.Add(detailsParam);

        con.Open();
        cmd.ExecuteNonQuery();
    }

    public List<VoucherWithDetails> GetAllVouchersWithDetails()
    {
        var list = new List<VoucherWithDetails>();

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        var query = @"
        SELECT v.Id, v.VoucherType, v.ReferenceNo, v.VoucherDate, v.CreatedBy,
               SUM(d.DebitAmount) AS TotalDebit, SUM(d.CreditAmount) AS TotalCredit
        FROM Vouchers v
        LEFT JOIN VoucherDetails d ON v.Id = d.VoucherId
        GROUP BY v.Id, v.VoucherType, v.ReferenceNo, v.VoucherDate, v.CreatedBy
        ORDER BY v.VoucherDate DESC";

        using var cmd = new SqlCommand(query, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new VoucherWithDetails
            {
                Id = reader.GetInt32(0),
                VoucherType = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1),
                ReferenceNo = reader.IsDBNull(2) ? "N/A" : reader.GetString(2),
                VoucherDate = reader.IsDBNull(3) ? DateTime.Today : reader.GetDateTime(3),
                CreatedBy = reader.IsDBNull(4) ? "System" : reader.GetString(4),
                TotalDebit = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                TotalCredit = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6)
            });
        }

        return list;
    }

    public void DeleteVoucher(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand("DELETE FROM VoucherDetails WHERE VoucherId = @Id; DELETE FROM Vouchers WHERE Id = @Id;", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }
}
