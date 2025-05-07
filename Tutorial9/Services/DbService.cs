using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Tutorial9.DTOs;

namespace Tutorial9.Services;

public class DbService : IDbService
{
    
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;
    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
   
    
   
    
    // public async Task DoSomethingAsync()
    // {
    //     await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
    //     await using SqlCommand command = new SqlCommand();
    //     
    //     command.Connection = connection;
    //     await connection.OpenAsync();
    //
    //     DbTransaction transaction = await connection.BeginTransactionAsync();
    //     command.Transaction = transaction as SqlTransaction;
    //
    //     // BEGIN TRANSACTION
    //     try
    //     {
    //         command.CommandText = "INSERT INTO Animal VALUES (@IdAnimal, @Name);";
    //         command.Parameters.AddWithValue("@IdAnimal", 1);
    //         command.Parameters.AddWithValue("@Name", "Animal1");
    //     
    //         await command.ExecuteNonQueryAsync();
    //     
    //         command.Parameters.Clear();
    //         command.CommandText = "INSERT INTO Animal VALUES (@IdAnimal, @Name);";
    //         command.Parameters.AddWithValue("@IdAnimal", 2);
    //         command.Parameters.AddWithValue("@Name", "Animal2");
    //     
    //         await command.ExecuteNonQueryAsync();
    //         
    //         await transaction.CommitAsync();
    //     }
    //     catch (Exception e)
    //     {
    //         await transaction.RollbackAsync();
    //         throw;
    //     }
    //     // END TRANSACTION
    // }

    public async Task ProcedureAsync()
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        command.CommandText = "NazwaProcedury";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@Id", 2);
        
        await command.ExecuteNonQueryAsync();
        
    }


    public async Task<bool> IfProductExistsAsync(int idProduct)
    {
        await using var con = new SqlConnection(_connectionString);
        await using var com = new SqlCommand();
        
        com.Connection = con;
        com.CommandText = "SELECT 1 FROM Product WHERE IdProduct = @id";
        com.Parameters.AddWithValue("@id", idProduct);
        await con.OpenAsync();
        var result = await com.ExecuteScalarAsync();
        return result != null;
    }

    public async Task<bool> IfWarehouseExistsAsync(int IdWarehouse)
    {
        await using var con = new SqlConnection(_connectionString);
        await using var com = new SqlCommand();
        
        com.Connection = con;
        com.CommandText = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @Id";
        com.Parameters.AddWithValue("@Id", IdWarehouse);
        await con.OpenAsync();
        var result = await com.ExecuteScalarAsync();
        return result != null;
    }

    public async Task<bool> IfProductInOrdersExists(int IdProduct, int Amount, DateTime date)
    {
        await using var con = new SqlConnection(_connectionString);
        await using var com = new SqlCommand();
        
        com.Connection = con;
        com.CommandText = "SELECT 1 FROM Order WHERE IdProduct = @idProduct AND Amount = @Amount AND CreatedAt < @Date";
        com.Parameters.AddWithValue("@idProduct", IdProduct);
        com.Parameters.AddWithValue("@Amount", Amount);
        com.Parameters.AddWithValue("@Date", date);
        await con.OpenAsync();
        var result = await com.ExecuteScalarAsync();
        return result != null;

    }


    // public async Task<bool> IfOrderRealizedAsync(int idOrder)
    // {
    //     await using var con = new SqlConnection(_connectionString);
    //     await using var com = new SqlCommand();
    //     
    //     com.Connection = con;
    //     com.CommandText = "SELECT 1 FROM Order WHERE IdProduct = @idProduct AND Amount = @Amount AND CreatedAt < @Date";
    //     com.Parameters.AddWithValue("@idProduct", IdProduct);
    //   
    //     await con.OpenAsync();
    //     var result = await com.ExecuteScalarAsync();
    //     return result != null;
    // }



    public async Task<(int id, string Error)> CreaateRecordAsync(InsertDataDto data)
    {
        
        string error =null;
        var productExists = await IfProductExistsAsync(data.IdProduct);
        var warehouseExists = await IfWarehouseExistsAsync(data.IdWarehouse);
        var productInOrdersExists = await IfProductInOrdersExists(data.IdProduct, data.Amount, data.CreatedAt);

        if (!productExists)
        {
            error = "Product already exists";
        }else if (!warehouseExists)
        {
            error = "Warehouse does not exist";
        } else if (!productInOrdersExists)
        {
            error = "Product in orders does not exist";    
        }

        if (error != null)
        {
            return (-1, error);
        }
        
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        try
        {

        }
        catch (SqlException ex)
        {
            command.CommandText = "INSERT INTO Animal VALUES (@IdAnimal, @Name);";
            command.Parameters.AddWithValue("@IdAnimal", 1);
            command.Parameters.AddWithValue("@Name", "Animal1");
            await transaction.RollbackAsync();
            throw;
        }

    }
 
}