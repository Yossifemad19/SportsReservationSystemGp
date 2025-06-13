using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Core.Interfaces;

namespace backend.Api.Services;

public class NoShowBackgroundService : BackgroundService
{
    private readonly ILogger<NoShowBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public NoShowBackgroundService(
        ILogger<NoShowBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NoShow Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Checking for no-shows...");
                
                using (var scope = _serviceProvider.CreateScope())
                {
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    await bookingService.HandleNoShowsAsync();
                }

                _logger.LogInformation("No-show check completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking for no-shows.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
} 