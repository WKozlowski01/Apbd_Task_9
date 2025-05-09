using Microsoft.AspNetCore.Mvc;
using Tutorial9.DTOs;
using Tutorial9.Services;

namespace Tutorial9.Controllers;

[ApiController]
[Route("api/warehouse")]
public class WarehouseController:ControllerBase
{
    
    private readonly IDbService _DbService;
 
    public WarehouseController(IDbService service)
    {
        _DbService = service;   
    }
    
    [HttpPost("first")]
    public async Task<IActionResult> CreaateRecordAsync(InsertDataDto data)
    {
        var result =await  _DbService.CreaateRecordAsync(data);
        if (result.id == -1)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.id);
    }
    
    
    [HttpPost("second")]
   
    public async Task<IActionResult> ProcedureAsync([FromBody] InsertDataDto data)
    {
        try
        {
            var result = await _DbService.ProcedureAsync(data);

            return result switch
            {
                DbResult.Ok         => Ok("Product added successfully."),
                DbResult.Created    => StatusCode(201, "Product added (new entry)."),
                DbResult.BadRequest => BadRequest("Invalid data."),
                DbResult.NotFound   => NotFound("Product or warehouse not found."),
                DbResult.Conflict   => Conflict("Order already realized."),
                DbResult.NotImpl    => StatusCode(501, "Feature not implemented."),
                _                   => StatusCode(500, "Unexpected server error.")
            };
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error.");
        }
    }
    
    
}