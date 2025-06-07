using APBD_DBFR.Models;
using APBD_DBFR.Services;
using Microsoft.EntityFrameworkCore;

namespace APBD_DBFR;

public class Program
{
        public static void Main(string[] args)
        {
                var builder = WebApplication.CreateBuilder(args);
                
                builder.Services.AddAuthorization();

                builder.Services.AddDbContext<ApbdDbfContext>(ord =>
                {
                        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                        ord.UseSqlServer(connectionString);
                });

                builder.Services.AddScoped<ITripsService, TripsService>();
                builder.Services.AddScoped<IClientsService, ClientsService>();
                builder.Services.AddControllers();
                builder.Services.AddOpenApi();

                var app = builder.Build();
                if (app.Environment.IsDevelopment())
                {
                        app.MapOpenApi();
                }
                app.UseHttpsRedirection();

                app.UseAuthorization();

                app.MapControllers();

                app.Run();
        }
}