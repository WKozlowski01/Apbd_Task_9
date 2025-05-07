using Microsoft.AspNetCore.Mvc;
using Tutorial9.DTOs;
using Tutorial9.Services;

namespace Tutorial9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController
{
    
    private readonly IDbService _DbService;
 
    public WarehouseController(IDbService service)
    {
        _DbService = service;   
    }
    
    [HttpPost]
    public async Task<IActionResult> AddClientAsync(InsertDataDto data)
    {
        var clients = await _DbService.CreaateClientAsync(client, cancelation);
        return Ok(clients);
    }
    
    
}