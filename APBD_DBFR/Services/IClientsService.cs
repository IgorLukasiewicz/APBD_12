namespace APBD_DBFR.Services;

public interface IClientsService
{
    public Task<(bool Success, string Message)> DeleteClient(int idClient);
    
}