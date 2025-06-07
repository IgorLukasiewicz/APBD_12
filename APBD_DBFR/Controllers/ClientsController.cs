using APBD_DBFR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientsService _clientsService;

    public ClientsController(IClientsService clientsService)
    {
        _clientsService = clientsService;
    }

    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var (success, message) = await _clientsService.DeleteClient(idClient);

        if (!success)
        {
            if (message.Contains("przypisane wycieczki"))
                return BadRequest(new { message });

            if (message.Contains("Nie znaleziono"))
                return NotFound(new { message });

            return BadRequest(new { message });
        }

        return Ok(new { message });
    }
}
