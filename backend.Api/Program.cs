
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
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;

namespace backend.Api;



public class Program
{
    
    public static async Task Main(string[] args)
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


        Env.Load(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, ".env"));
        Console.WriteLine(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, ".env"));

        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        var host = builder.Environment.IsDevelopment() ? "localhost":Environment.GetEnvironmentVariable("HOST");
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB");
        
        if (string.IsNullOrEmpty(postgresUser) || string.IsNullOrEmpty(postgresPassword) ||string.IsNullOrEmpty(host) ||string.IsNullOrEmpty(database))
        {
            throw new Exception("Missing environment variables.");
        }

// Get the connection string from configuration
        var rawConnectionString = builder.Configuration.GetConnectionString("PostgreSql");

// Replace placeholders correctly
        var connectionString = rawConnectionString!
            .Replace("${HOST}", host)
            .Replace("${POSTGRES_DB}", database)
            .Replace("${POSTGRES_USER}", postgresUser)
            .Replace("${POSTGRES_PASSWORD}", postgresPassword);

        Console.WriteLine($"Final Connection String: {connectionString}");



        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        
        // configure auto mapper
        builder.Services.AddAutoMapper(typeof(Program).Assembly);
        



        // Add services to the container.

        builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));

        builder.Services.AddScoped<ITokenService, TokenService>();

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IFacilityService, FacilityService>();





        builder.Services.AddControllers()
            .AddFluentValidation(f => {
                f.RegisterValidatorsFromAssemblyContaining<RegisterDto>();
            })
            ;
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

            // Add JWT Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Enter 'Bearer {your-token}' to authenticate.",
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        // if (app.Environment.IsDevelopment())
        // {
        //     app.UseSwagger();
        //     app.UseSwaggerUI();
        // }
        
        app.UseSwagger();
        app.UseSwaggerUI();
        
        
        app.UseHttpsRedirection();

        app.UseAuthentication();
        
        app.UseAuthorization();


        app.MapControllers();
        
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();
            logger.LogInformation( "database updated");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during migration");
        }


        app.Run();
    }
}
