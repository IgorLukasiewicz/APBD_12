using APBD_DBFR.DTOs;
using APBD_DBFR.Models;

namespace APBD_DBFR.Services;

public interface ITripsService
{
  public  Task<TripsPagesDTO> tripsTask(int page = 1, int pageSize = 10);
  public Task<(bool Success, string Message)> AssignClientToTrip(AddedClientDTO newClient);


}