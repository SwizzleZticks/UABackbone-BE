
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.Models;

namespace UABackbone_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("UA_DB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("No connection string passed in Environment Variables");
            }
            
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<RailwayContext>(
                option => option.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
