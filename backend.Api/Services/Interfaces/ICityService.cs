namespace backend.Api.Services.Interfaces;

public interface ICityService
{
    IReadOnlyCollection<string> GetAllAllowedCities();
}

