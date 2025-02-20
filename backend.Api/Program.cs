
using System.Text;
using backend.Api.DTOs;
using backend.Api.Services;
using backend.Core.Interfaces;
using backend.Repository.Data;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation.AspNetCore;
using backend.Api.Helpers;

namespace backend.Api;



public class Program
{
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwt:SecretKey"])),
                    ValidIssuer = builder.Configuration["jwt:Issuer"],
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["jwt:Audience"],
                    ValidateLifetime=true,
                });
        
        
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
        
        
        // configure auto mapper
        builder.Services.AddAutoMapper(typeof(Program).Assembly);
        



        // Add services to the container.

        builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));

        builder.Services.AddScoped<ITokenService, TokenService>();

        builder.Services.AddScoped<IAuthService, AuthService>();

        



        builder.Services.AddControllers()
            .AddFluentValidation(f => {
                f.RegisterValidatorsFromAssemblyContaining<RegisterDto>();
            })
            ;
        
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
