using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CityInfo.Api.DbContexts;
using CityInfo.Api.Entities;
using CityInfo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.Api.Services;

public class CityInfoRepository(CityInfoContext context) : ICityInfoRepository
{
  public async Task<IEnumerable<City>> GetCitiesAsync(CancellationToken cancellationToken)
  {
    return await context.Cities.OrderBy(c => c.Name).ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<City>> GetCitiesReadOnlyAsync(CancellationToken cancellationToken)
  {
    return await context.Cities.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);
  }

  public async Task<(IEnumerable<City>, PaginationMetadata?)> GetCitiesReadOnlyAsync(string? name, string? searchQuery,
    int pageNumber, int pageSize,
    CancellationToken cancellationToken)
  {
    // if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(searchQuery))
    // {
    //   return await GetCitiesReadOnlyAsync(cancellationToken);
    // }

    var collection = context.Cities as IQueryable<City>;

    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      collection = collection.Where(c => c.Name == name);
    }

    if (!string.IsNullOrWhiteSpace(searchQuery))
    {
      searchQuery = searchQuery.Trim();
      collection = collection.Where(c => c.Name.Contains(searchQuery)
      || (c.Description != null && c.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)));
    }

    var totalItemCount = await collection.CountAsync(cancellationToken);
    var metadata = new PaginationMetadata(totalItemCount, pageSize, pageNumber);

    var collectionResult = await collection.AsNoTracking()
      .OrderBy(c => c.Name)
      .Skip((pageNumber - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);

    return (collectionResult, metadata);
  }

  public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest, CancellationToken cancellationToken)
  {
    if (includePointsOfInterest)
    {
      return await context.Cities.Include(c => c.PointsOfInterest).FirstOrDefaultAsync(c => c.Id == cityId, cancellationToken);
    }
    return await context.Cities.FirstOrDefaultAsync(c => c.Id == cityId, cancellationToken);
  }

  public async Task<bool> CityExistsAsync(int cityId, CancellationToken cancellationToken)
  {
    return await context.Cities.AnyAsync(c => c.Id == cityId, cancellationToken);
  }

  public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId, CancellationToken cancellationToken)
  {
    return await context.PointsOfInterest.FirstOrDefaultAsync(p => p.Id == pointOfInterestId && p.CityId == cityId, cancellationToken);
  }

  public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId, CancellationToken cancellationToken)
  {
    return await context.PointsOfInterest.Where(p => p.CityId == cityId).OrderBy(p => p.Name).ToListAsync(cancellationToken);
  }

  public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest, CancellationToken cancellationToken)
  {
    var city = await GetCityAsync(cityId, false, cancellationToken);
    city?.PointsOfInterest.Add(pointOfInterest);
  }

  public void DeletePointOfInterest(PointOfInterest pointOfInterest, CancellationToken cancellationToken)
  {
    context.PointsOfInterest.Remove(pointOfInterest);
  }

  public async Task<int> UpdatePointsOfInterestDescriptionForCityAsync(int cityId, string newDescription, CancellationToken cancellationToken)
  {
    return await context.PointsOfInterest
      .Where(p => p.CityId == cityId)
      .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Description, newDescription), cancellationToken);
  }

  public async Task<int> DeleteAllPointsOfInterestForCityAsync(int cityId, CancellationToken cancellationToken)
  {
    throw new System.NotImplementedException();
  }

  public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
  {
    return (await context.SaveChangesAsync(cancellationToken) >= 0);
  }
}
