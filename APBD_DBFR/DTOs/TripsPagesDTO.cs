using System.Text.Json.Serialization;

namespace APBD_DBFR.DTOs;

public class TripsPagesDTO
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public List<TripInDTO> AllTrips { get; set; }
}
public class TripInDTO
{
    [JsonIgnore]
    public int IdTrip { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<CountryDTO> Countries { get; set; }
    public List<ClientInTripsListDTO> Clients { get; set; }
}

