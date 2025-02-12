
using backend.Core.Interfaces;
using backend.Repository.Data;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

namespace backend.Api;



public class Program
{
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        Env.Load(Path.Combine(Directory.GetParent(
            Directory.GetCurrentDirectory())!.FullName,".env"));
        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        if (string.IsNullOrEmpty(postgresUser) || string.IsNullOrEmpty(postgresPassword))
        {
            throw new Exception("Missing environment variables.");
        }
        
        var connectionString = builder.Configuration.GetConnectionString("PostgreSql")!
            .Replace("{POSTGRES_USER}", Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "default_user")
            .Replace("{POSTGRES_PASSWORD}", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "default_password");
        Console.WriteLine($"Connection string: {connectionString}");
        
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        // Add services to the container.
        
        builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
        
        
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
