using APBD_DBFR.Services;
using Microsoft.Data.SqlClient;

public class ClientsService : IClientsService
{
    private readonly string _connectionString;

    public ClientsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<(bool Success, string Message)> DeleteClient(int idClient)
    {
        await using var con = new SqlConnection(_connectionString);
        await con.OpenAsync();

        var query1 = "SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @idClient";

        var checkTripsCommand = new SqlCommand(query1, con);
        checkTripsCommand.Parameters.AddWithValue("@idClient", idClient);

        int tripsCount = (int)await checkTripsCommand.ExecuteScalarAsync();

        if (tripsCount > 0)
        {
            return (false, "Do tego klienta sa przypisane wycieczki, wiec nie moze zostac usuniety");
        }

        var query2 = "DELETE FROM Client WHERE IdClient = @idClient";

        var deleteClientCommand = new SqlCommand(query2, con);
        
        deleteClientCommand.Parameters.AddWithValue("@idClient", idClient);

        int rowsAffected = await deleteClientCommand.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
        {
            return (false, "Taki klient nie istnieje");
        }

        return (true, "Klient usuniety");
    }
}