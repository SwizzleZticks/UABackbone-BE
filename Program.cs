using UABackbone_Backend.Extensions;


namespace UABackbone_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.AddIdentityServices(builder.Configuration);
            builder.Services.AddEmailService();
            builder.Services.AddIdentityService();
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200", "http://uabackbone-be.up.railway.app")
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseDeveloperExceptionPage(); // Only use in dev

            app.UseSwagger(); 
            app.UseSwaggerUI();

            app.UseCors();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAuthorization();
            app.MapControllers();
            
            app.Run();
        }
    }
}
