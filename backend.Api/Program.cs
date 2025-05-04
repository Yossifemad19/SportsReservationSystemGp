using System.IdentityModel.Tokens.Jwt;
using System.Text;
using backend.Api.DTOs;
using backend.Api.Errors;
using backend.Api.Services;
using backend.Core.Interfaces;
using backend.Repository.Data;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation.AspNetCore;
using backend.Api.Helpers;
using backend.Api.Middlewares;
using backend.Infrastructure.Data;
using backend.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using backend.Api.Services.Interfaces;

namespace backend.Api;



public class Program
{
    
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
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
        builder.Services.AddScoped<IBookingRepository, BookingRepository>();
        builder.Services.AddScoped<IBookingService, BookingService>();
        builder.Services.AddScoped<IAdminService, AdminService>();
        builder.Services.AddScoped<IEmailService, SendGridEmailService>();

        builder.Services.AddScoped<IAdminService, AdminService>();  
        //builder.Services.AddScoped<IAIChatService, AIChatService>();
        //builder.Services.AddScoped<IMatchingService, MatchingService>();
        builder.Services.AddScoped<IMatchService, MatchService>();
        builder.Services.AddLogging();





        builder.Services.AddControllers()
            .AddFluentValidation(f => {
                f.RegisterValidatorsFromAssemblyContaining<RegisterDto>();
            })
            .AddJsonOptions(options =>
            {
                // Change this from Preserve to IgnoreCycles to avoid the $id and $values in the response
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.MaxDepth = 64; // Keep this setting
            });
        
        builder.Services.Configure<ApiBehaviorOptions>(o =>
            {
                o.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(e => e.Value.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
        
                    return new BadRequestObjectResult(new ApiValidation()
                    {
                        Errors = errors
                    });
        
                };
            }
        );
        
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

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
        
                });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        // if (app.Environment.IsDevelopment())
        // {
        //     app.UseSwagger();
        //     app.UseSwaggerUI();
        // }

        app.UseCors("AllowAll");


        app.UseSwagger();
        app.UseSwaggerUI();
        
        
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseStatusCodePagesWithReExecute("/Errors/{0}");
        app.UseMiddleware<ExceptionMiddleware>();
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
            logger.LogInformation("Database updated");

            var unitOfWork = services.GetRequiredService<IUnitOfWork>();

            // Seed Admin Data
            var adminResult = await SeedAdmin.SeedAdminData(unitOfWork, AuthService.GetHashedPassword("Admin@123"));
            if (adminResult > 0)
                logger.LogInformation("Successfully seeded admin data");
            else if (adminResult == 0)
                logger.LogInformation("Admin data seeding failed");
            else
                logger.LogError("Admin data was not seeded");

            // Seed Owners Data 
            var ownerPasswordHash = AuthService.GetHashedPassword("Owner@123");
            var ownerResult = await SeedOwners.SeedOwnersData(unitOfWork, ownerPasswordHash);

            if (ownerResult > 0)
                logger.LogInformation("Successfully seeded owners data");
            else if (ownerResult == 0)
                logger.LogInformation("Owners data seeding failed");
            else
                logger.LogError("Owners data was not seeded");

            // Seed Facilities Data
            var facilitiesResult = await SeedFacilities.SeedFacilitiesData(unitOfWork);
            if (facilitiesResult > 0)
                logger.LogInformation("Successfully seeded owners data");
            else if (facilitiesResult == 0)
                logger.LogInformation("Owners data seeding failed");
            else
                logger.LogError("Owners data was not seeded");
            // seed sports data
            var sportResult = await SeedSports.SeedSportsData(unitOfWork);
            if (sportResult > 0)
                logger.LogInformation("Successfully seeded sports data");
            else if (sportResult == 0)
                logger.LogInformation("Sports already seeded");
            else
                logger.LogError("Error occurred while seeding sports");


            var courtResult = await SeedCourts.SeedCourtsData(unitOfWork);
            if (courtResult > 0)
                logger.LogInformation("Successfully seeded courts data");
            else if (courtResult == 0)
                logger.LogInformation("Courts already seeded");
            else
                logger.LogError("Error occurred while seeding courts");

            // Add this after the other seed calls
            var roleResult = await SeedUserRoles.SeedRoles(unitOfWork);
            if (roleResult > 0)
                logger.LogInformation("Successfully seeded user roles");
            else if (roleResult == 0)
                logger.LogInformation("User roles already seeded");
            else
                logger.LogError("Error occurred while seeding user roles");

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during migration and seeding");
        }

       

    app.Run();
    }
}
