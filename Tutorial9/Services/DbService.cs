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
        com.CommandText = "SELECT 1 FROM [Order] WHERE IdProduct = @idProduct AND Amount = @Amount";
        com.Parameters.AddWithValue("@idProduct", IdProduct);
        com.Parameters.AddWithValue("@Amount", Amount);
        await con.OpenAsync();
        var result = await com.ExecuteScalarAsync();
        return result != null;

    }


    public async Task<bool> IfOrderRealizedAsync(int IdProduct, int Amount, DateTime date)
    {


        await using var con = new SqlConnection(_connectionString);
        await using var com = new SqlCommand();
        
        com.Connection = con;
        com.CommandText = "SELECT IdOrder FROM [Order] WHERE IdProduct = @idProduct AND Amount = @Amount";
        com.Parameters.AddWithValue("@idProduct", IdProduct);
        com.Parameters.AddWithValue("@Amount", Amount);
      
        await con.OpenAsync();
        var order = await com.ExecuteScalarAsync();
        



        await using var com2 = new SqlCommand();
        com2.Connection = con;
        com2.CommandText = "SELECT 1 FROM Product_Warehouse WHERE IdOrder = @order";
        com2.Parameters.AddWithValue("@order", order);
        var result = await com2.ExecuteScalarAsync(); 
        return result == null;
    }



    public async Task<(int id, string Error)> CreaateRecordAsync(InsertDataDto data)
    {
        
        string error =null;
        var productExists = await IfProductExistsAsync(data.IdProduct);
        if (!productExists)
        {
            error = "Product does not exists";
            return (-1, error);
        }
        
        var warehouseExists = await IfWarehouseExistsAsync(data.IdWarehouse);
        if (!warehouseExists)
        {
            error = "Warehouse does not exist";
            return (-1, error);
        }
        
        
        var productInOrdersExists = await IfProductInOrdersExists(data.IdProduct, data.Amount, data.CreatedAt);
        if (!productInOrdersExists)
        {
            error = "Product in orders does not exist";
            return (-1, error);
        }
        
        
        
        var orderRealized = await IfOrderRealizedAsync(data.IdProduct, data.Amount, data.CreatedAt);
        if (!orderRealized)
        {
            error = "Order already realised";
            return (-1, error);
        }
        
        
        
        await using var con = new SqlConnection(_connectionString);
        await using var command = new SqlCommand();
        command.Connection = con;
        await con.OpenAsync();
        DbTransaction transaction = await con.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        try
        {
            command.CommandText = "UPDATE [Order] SET FulfilledAt = @FullfilledAt WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @FullfilledAt;SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @FullfilledAt";
            command.Parameters.AddWithValue("@FullfilledAt", DateTime.Now);
            command.Parameters.AddWithValue("@IdProduct", data.IdProduct);
            command.Parameters.AddWithValue("@Amount", data.Amount);
            var orderId = await command.ExecuteScalarAsync();
            
            await using var command3 = new SqlCommand();
            command3.Transaction = transaction as SqlTransaction;
            command3.Connection = con;
            command3.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct;";
            command3.Parameters.AddWithValue("@IdProduct", data.IdProduct);
            var price =  await command3.ExecuteScalarAsync();
            
            
            
            await using var command2 = new SqlCommand();
            command2.Transaction = transaction as SqlTransaction;
            command2.Connection = con;
            command2.CommandText = @"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder,Amount,Price,CreatedAt) 
                                    VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
                                     SELECT IdProductWarehouse FROM Product_Warehouse WHERE IdProduct = @IdProduct AND IdWarehouse = @IdWarehouse AND IdProduct = @IdProduct AND IdOrder = @IdOrder AND CreatedAt = @CreatedAt";
            
            command2.Parameters.AddWithValue("@IdWarehouse", data.IdWarehouse);
            command2.Parameters.AddWithValue("@IdProduct", data.IdProduct);
            command2.Parameters.AddWithValue("@IdOrder", orderId);
            command2.Parameters.AddWithValue("@Amount", data.Amount);
            command2.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            command2.Parameters.AddWithValue("Price", data.Amount*(decimal)price);
         
            var result = await command2.ExecuteScalarAsync();
            
         
            await transaction.CommitAsync();
            return ((int id, string Error))(result,error);


         



        }
        catch (SqlException ex)
        {
            await transaction.RollbackAsync(); 
            throw;
        }

    }
 
}