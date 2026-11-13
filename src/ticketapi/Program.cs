
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using ticketapi.Daten;

namespace ticketapi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

   
            builder.Services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<TicketDatenbank>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("TicketDb")));

            builder.Services.AddCors(options =>
            options.AddPolicy("Frontend", policy => policy
            .WithOrigins("http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()));

            var app = builder.Build();

           
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TicketDatenbank>();
                db.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseCors("Frontend");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
