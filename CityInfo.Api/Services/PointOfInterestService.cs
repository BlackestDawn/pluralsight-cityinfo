using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CityInfo.Api.Entities;
using CityInfo.Api.Models;

namespace CityInfo.Api.Services;

public class PointOfInterestService(ICityInfoRepository repository,
  IMapper mapper,
  IMailService mailService) : IPointofInterestService
{
  public async Task<PointOfInterestCreationResult> CreatePointOfInterestAsync(int cityId,
    PointOfInterestForCreationDto pointOfInterest,
    CancellationToken cancellationToken)
  {
    if (!await repository.CityExistsAsync(cityId, cancellationToken))
    {
      return PointOfInterestCreationResult.Failed("City not found");
    }

    // TODO: optimize by only returning count
    var existingPOIs = await repository.GetPointsOfInterestForCityAsync(cityId, cancellationToken);
    if (existingPOIs.Count() >= 10)
    {
      return PointOfInterestCreationResult.Failed("City has reached max capacity of 10 points of interest");
    }

    var pointOfInterestEntity = mapper.Map<PointOfInterest>(pointOfInterest);
    await repository.AddPointOfInterestForCityAsync(cityId, pointOfInterestEntity, cancellationToken);
    await repository.SaveChangesAsync(cancellationToken);

    await SendCreationNotificationsAsync(cityId, pointOfInterestEntity);

    return PointOfInterestCreationResult.Successful(mapper.Map<PointOfInterestDto>(pointOfInterestEntity));
  }

  private async Task SendCreationNotificationsAsync(int cityId, PointOfInterest pointOfInterest)
  {
    mailService.SendMail(
      "Point of interest created",
      $"Point of interest {pointOfInterest.Name} has been created in city with id {cityId}."
    );
  }
}
