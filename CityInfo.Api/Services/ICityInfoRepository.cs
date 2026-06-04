using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CityInfo.Api.Entities;
using CityInfo.Api.Models;

namespace CityInfo.Api.Services;

public interface ICityInfoRepository
{
  Task<IEnumerable<City>> GetCitiesAsync(CancellationToken cancellationToken);
  Task<IEnumerable<City>> GetCitiesReadOnlyAsync(CancellationToken cancellationToken);
  Task<(IEnumerable<City>, PaginationMetadata?)> GetCitiesReadOnlyAsync(string? name, string? searchQuery,
    int pageNumber, int pageSize,
    CancellationToken cancellationToken);
  Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest, CancellationToken cancellationToken);
  Task<bool> CityExistsAsync(int cityId, CancellationToken cancellationToken);
  Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId, CancellationToken cancellationToken);
  Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId, CancellationToken cancellationToken);
  Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest, CancellationToken cancellationToken);
  void DeletePointOfInterest(PointOfInterest pointOfInterest, CancellationToken cancellationToken);
  Task<int> UpdatePointsOfInterestDescriptionForCityAsync(int cityId, string newDescription, CancellationToken cancellationToken);
  Task<int> DeleteAllPointsOfInterestForCityAsync(int cityId, CancellationToken cancellationToken);
  Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}
