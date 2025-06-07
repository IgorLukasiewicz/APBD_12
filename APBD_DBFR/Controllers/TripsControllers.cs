using APBD_DBFR.DTOs;
using APBD_DBFR.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_DBFR.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripsService _tripsService;

    public TripsController(ITripsService tripsService)
    {
        _tripsService = tripsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _tripsService.tripsTask();

        return Ok(trips);
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(AddedClientDTO newClient)
    {
        var (success, message) = await _tripsService.AssignClientToTrip(newClient);

        if (!success)
        {
            if (message.Contains("już istnieje"))
            {
                return BadRequest(new { message });
            } 
            if (message.Contains("jest już przypisany"))
            {
                return BadRequest(new { message });
            } 
            if (message.Contains("o podanym Id nie"))
            {
                return BadRequest(new { message });
            } 
            if(message.Contains("już się odbyła"))
            {
                return BadRequest(new { message });
            }
        }

        return Ok(new { message });
    }

    
}