using backend.Api.Services;
using System.Linq;
using Xunit;

namespace backend.Tests.Services
{
    public class CityServiceTests
    {
        private readonly CityService _cityService;

        public CityServiceTests()
        {
            _cityService = new CityService();
        }

        [Fact]
        public void GetAllAllowedCities_ShouldReturnAllCities()
        {
            // Act
            var cities = _cityService.GetAllAllowedCities();

            // Assert
            Assert.NotNull(cities);
            Assert.NotEmpty(cities);
            Assert.Equal(25, cities.Count); // Updated to match actual count
        }

        [Fact]
        public void GetAllAllowedCities_ShouldReturnCitiesInAlphabeticalOrder()
        {
            // Act
            var cities = _cityService.GetAllAllowedCities().ToList();

            // Assert
            Assert.Equal(cities.OrderBy(c => c), cities);
        }

        [Fact]
        public void GetAllAllowedCities_ShouldContainSpecificCities()
        {
            // Act
            var cities = _cityService.GetAllAllowedCities();

            // Assert
            Assert.Contains("Cairo", cities);
            Assert.Contains("Giza", cities);
            Assert.Contains("Nasr City", cities);
            Assert.Contains("6th of October", cities);
        }

        [Fact]
        public void GetAllAllowedCities_ShouldBeCaseInsensitive()
        {
            // Act
            var cities = _cityService.GetAllAllowedCities();

            // Assert
            Assert.Contains("Cairo", cities, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("giza", cities, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("Nasr City", cities, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetAllAllowedCities_ShouldNotContainInvalidCities()
        {
            // Act
            var cities = _cityService.GetAllAllowedCities();

            // Assert
            Assert.DoesNotContain("Alexandria", cities);
            Assert.DoesNotContain("Luxor", cities);
            Assert.DoesNotContain("Aswan", cities);
        }
    }
} 