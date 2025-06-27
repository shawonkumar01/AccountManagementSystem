using AccountManagementSystem.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;

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

        // Add parameters for the main voucher
        cmd.Parameters.AddWithValue("@VoucherType", model.VoucherType ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ReferenceNo", model.ReferenceNo ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@VoucherDate", model.VoucherDate);
        cmd.Parameters.AddWithValue("@CreatedBy", createdBy ?? (object)DBNull.Value);

        // Create a DataTable for the voucher details (entries)
        var dt = new DataTable();
        dt.Columns.Add("AccountId", typeof(int));
        dt.Columns.Add("DebitAmount", typeof(decimal));
        dt.Columns.Add("CreditAmount", typeof(decimal));

        // Populate the DataTable with the entries
        foreach (var line in model.Entries)
        {
            dt.Rows.Add(line.AccountId, line.DebitAmount, line.CreditAmount);
        }

        // Add the DataTable as a parameter for the stored procedure
        var detailsParam = new SqlParameter("@VoucherDetailsTbl", SqlDbType.Structured)
        {
            Value = dt,
            TypeName = "VoucherDetailsType" // This must match the SQL type definition
        };
        cmd.Parameters.Add(detailsParam);

        // Open the connection and execute the stored procedure
        con.Open();
        cmd.ExecuteNonQuery();
    }

    public List<Voucher> GetAllVouchers()
    {
        var vouchers = new List<Voucher>();

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand("SELECT Id, VoucherType, ReferenceNo, VoucherDate, CreatedBy FROM Vouchers", conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var voucher = new Voucher
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                VoucherType = reader.IsDBNull(reader.GetOrdinal("VoucherType"))
                    ? "N/A"
                    : reader.GetString(reader.GetOrdinal("VoucherType")),

                ReferenceNo = reader.IsDBNull(reader.GetOrdinal("ReferenceNo"))
                    ? "N/A"
                    : reader.GetString(reader.GetOrdinal("ReferenceNo")),

                VoucherDate = reader.IsDBNull(reader.GetOrdinal("VoucherDate"))
                    ? DateTime.MinValue
                    : reader.GetDateTime(reader.GetOrdinal("VoucherDate")),

                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy"))
                    ? "Unknown"
                    : reader.GetString(reader.GetOrdinal("CreatedBy"))
            };

            vouchers.Add(voucher);
        }

        return vouchers;
    }

}
