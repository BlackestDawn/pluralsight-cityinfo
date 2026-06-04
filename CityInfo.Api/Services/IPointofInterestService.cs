using System.Threading;
using System.Threading.Tasks;
using CityInfo.Api.Models;

namespace CityInfo.Api.Services;

public interface IPointofInterestService
{
  Task<PointOfInterestCreationResult> CreatePointOfInterestAsync(int cityId,
    PointOfInterestForCreationDto pointOfInterest,
    CancellationToken cancellationToken);
}
