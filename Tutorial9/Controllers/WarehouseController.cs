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
    
    [HttpPost]
    public async Task<IActionResult> CreaateRecordAsync(InsertDataDto data)
    {
        var result =await  _DbService.CreaateRecordAsync(data);
        if (result.id == -1)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.id);
    }
    
    
}