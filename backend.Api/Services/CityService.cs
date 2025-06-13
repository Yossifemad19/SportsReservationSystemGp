using backend.Api.Services.Interfaces;

namespace backend.Api.Services;


public class CityService : ICityService
{
    private static readonly HashSet<string> _allowedCities = new(StringComparer.OrdinalIgnoreCase)
    {
        
        "Cairo", 
        "Giza", 
    };

    public IReadOnlyCollection<string> GetAllAllowedCities()
        => _allowedCities.OrderBy(c => c).ToList().AsReadOnly();
}

