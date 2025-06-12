using backend.Api.Services.Interfaces;

namespace backend.Api.Services;


public class CityService : ICityService
{
    private static readonly HashSet<string> _allowedCities = new(StringComparer.OrdinalIgnoreCase)
    {
        // Cairo
        "Cairo", "Nasr City", "Heliopolis", "New Cairo", "Maadi", "Zamalek",
        "Downtown", "El Marg", "Ain Shams", "El Shorouk", "Mokattam", "Obour", "Badr", "Fifth Settlement",
        // Giza
        "Giza", "Dokki", "Mohandessin", "Agouza", "6th of October", "Sheikh Zayed",
        "Faisal", "Haram", "Imbaba", "Warraq", "Boulak El Dakrour"
    };

    public IReadOnlyCollection<string> GetAllAllowedCities()
        => _allowedCities.OrderBy(c => c).ToList().AsReadOnly();
}

