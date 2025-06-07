using APBD_DBFR.DTOs;
using APBD_DBFR.Models;
using Microsoft.Data.SqlClient;

namespace APBD_DBFR.Services;

public class TripsService : ITripsService
{
    
    private readonly string _connectionString;

    public TripsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
  public async Task<TripsPagesDTO> tripsTask(int page = 1, int pageSize = 10)
{
    var tripsDict = new Dictionary<int, TripInDTO>();

    var query = @"
       SELECT 
    t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
    c.Name AS CountryName,
    cl.FirstName, cl.LastName
FROM Trip t
LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
LEFT JOIN Country c ON ct.IdCountry = c.IdCountry
LEFT JOIN Client_Trip clt ON t.IdTrip = clt.IdTrip
LEFT JOIN Client cl ON clt.IdClient = cl.IdClient
ORDER BY t.DateTo DESC

    ";

    await using var conn = new SqlConnection(_connectionString);
    await using var cmd = new SqlCommand(query, conn);
    await conn.OpenAsync();

    var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        int idTrip = reader.GetInt32(0);

        if (!tripsDict.TryGetValue(idTrip, out var tripDto))
        {
            tripDto = new TripInDTO
            {
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                Countries = new List<CountryDTO>(),
                Clients = new List<ClientInTripsListDTO>()
            };
            tripsDict[idTrip] = tripDto;
        }

        if (!reader.IsDBNull(6))
            tripDto.Countries.Add(new CountryDTO { Name = reader.GetString(6) });

        if (!reader.IsDBNull(7) && !reader.IsDBNull(8))
            tripDto.Clients.Add(new ClientInTripsListDTO
            {
                FirstName = reader.GetString(7),
                LastName = reader.GetString(8)
            });
    }
    
    var allTrips = tripsDict.Values.ToList();

    var pagedTrips = allTrips
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return new TripsPagesDTO
    {
        PageNumber = page,
        PageSize = pageSize,
        AllTrips = pagedTrips
    };
}

public async Task<(bool Success, string Message)> AssignClientToTrip(AddedClientDTO newClient)
{
    await using var con = new SqlConnection(_connectionString);
    await con.OpenAsync();

    
    var query1 = "SELECT COUNT(*) FROM Client WHERE Pesel = @Pesel";
    var check1Command = new SqlCommand(query1, con);
    check1Command.Parameters.AddWithValue("@Pesel", newClient.Pesel);
    int clientExists = (int)await check1Command.ExecuteScalarAsync();
    if (clientExists > 0)
    {
        return (false, "Ten klient już istnieje");
    }

  
    var query2 = @"
        SELECT COUNT(*) 
        FROM Client_Trip CT
        JOIN Client C ON CT.IdClient = C.IdClient
        WHERE CT.IdTrip = @TripId AND C.Pesel = @Pesel";
    var check2Command = new SqlCommand(query2, con);
    check2Command.Parameters.AddWithValue("@TripId", newClient.IdTrip);
    check2Command.Parameters.AddWithValue("@Pesel", newClient.Pesel);
    int clientInTrip = (int)await check2Command.ExecuteScalarAsync();
    if (clientInTrip > 0)
    {
        return (false, "Ten klient jest juz przypisany do tej wycieczki");
    }

 
    var query3 = "SELECT DateFrom FROM Trip WHERE IdTrip = @TripId";
    var check3Command = new SqlCommand(query3, con);
    check3Command.Parameters.AddWithValue("@TripId", newClient.IdTrip);
    var tripDateObj = await check3Command.ExecuteScalarAsync();
    if (tripDateObj == null)
    {
        return (false, "Ta wycieczka nie istnieje");
    }
    DateTime tripDateFrom = (DateTime)tripDateObj;
    if (tripDateFrom <= DateTime.Now)
    {
        return (false, "Ta wycieczka już się odbyla");
    }

   
    var insertClientQuery = @"
        INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) 
        VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
        SELECT SCOPE_IDENTITY();";
    var insertClientCmd = new SqlCommand(insertClientQuery, con);
    insertClientCmd.Parameters.AddWithValue("@FirstName", newClient.FirstName);
    insertClientCmd.Parameters.AddWithValue("@LastName", newClient.LastName);
    insertClientCmd.Parameters.AddWithValue("@Email", newClient.Email);
    insertClientCmd.Parameters.AddWithValue("@Telephone", newClient.Telephone);
    insertClientCmd.Parameters.AddWithValue("@Pesel", newClient.Pesel);
    var newClientIdObj = await insertClientCmd.ExecuteScalarAsync();
    int newClientId = Convert.ToInt32(newClientIdObj);

    var insertClientTripQuery = @"
        INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
        VALUES (@IdClient, @IdTrip, @RegisteredAt, NULL)";
    var insertClientTripCmd = new SqlCommand(insertClientTripQuery, con);
    insertClientTripCmd.Parameters.AddWithValue("@IdClient", newClientId);
    insertClientTripCmd.Parameters.AddWithValue("@IdTrip", newClient.IdTrip);
    insertClientTripCmd.Parameters.AddWithValue("@RegisteredAt", DateTime.Now);

    await insertClientTripCmd.ExecuteNonQueryAsync();

    return (true, "Klient zostal przypisany do wycieczki");
    }
}
