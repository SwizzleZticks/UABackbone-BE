
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UABackbone_Backend.Extensions;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;
using UABackbone_Backend.Services;

namespace UABackbone_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplicationServices(builder.Configuration); //W black hole
            builder.Services.AddIdentityServices(builder.Configuration); //another W black hole
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:8080")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowAnyOrigin();
                    });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors();
            app.UseAuthorization();
            app.UseAuthentication();
            app.UseAuthorization();

            // Listen on port 80 inside the container
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            app.Run($"http://0.0.0.0:{port}");
        }

    }
}
