using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CityInfo.Api.Entities;
using CityInfo.Api.Models;
using CityInfo.Api.Services;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CityInfo.Api.Controllers;

[ApiController]
[Route("api/cities/{cityId}/pointsofinterest")]
public class PointsOfInterestController(ILogger<PointsOfInterestController> logger,
  IMailService mailService,
  ICityInfoRepository cityInfoRepository,
  IMapper mapper,
  IPointofInterestService pointOfInterestService) : ControllerBase
{
  [HttpGet]
  public async Task<IActionResult> GetPointsOfInterest(int cityId,
    CancellationToken cancellationToken = default)
  {
    try
    {
      if (!await cityInfoRepository.CityExistsAsync(cityId, cancellationToken))
      {
        logger.LogInformation("City with id {cityId} not found when accessing point of interest.", cityId);
        return NotFound();
      }

      var pointsOfInterest = await cityInfoRepository.GetPointsOfInterestForCityAsync(cityId, cancellationToken);

      return Ok(mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterest));
    }
    catch (Exception ex)
    {
      logger.LogCritical(ex, "Exception while getting points of interest for city with id {cityId}.", cityId);
      return StatusCode(500, "A problem occured while handling your request.");
    }
  }

  [HttpGet("{pointOfInterestId}", Name = "GetPointOfInterest")]
  public async Task<IActionResult> GetPointOfInterest(int cityId, int pointOfInterestId,
    CancellationToken cancellationToken = default)
  {
    if (!await cityInfoRepository.CityExistsAsync(cityId, cancellationToken))
    {
      logger.LogInformation("City with id {cityId} not found when accessing point of interest.", cityId);
      return NotFound();
    }

    var pointOfInterest = await cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId, cancellationToken);
    if (pointOfInterest == null)
    {
      return NotFound();
    }

    return Ok(mapper.Map<PointOfInterestDto>(pointOfInterest));
  }

  [HttpPost]
  public async Task<IActionResult> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest,
    CancellationToken cancellationToken = default)
  {
    var result = await pointOfInterestService.CreatePointOfInterestAsync(cityId, pointOfInterest, cancellationToken);
    if (!result.Success)
    {
      return BadRequest(result.ErrorMessage);
    }

    return CreatedAtRoute("GetPointOfInterest", new
    {
      cityId,
      pointOfInterestId = result.PointOfInterest!.Id
    },
      result.PointOfInterest
    );
  }

  [HttpDelete("{pointOfInterestId}")]
  public async Task<IActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId,
    CancellationToken cancellationToken = default)
  {
    if (!await cityInfoRepository.CityExistsAsync(cityId, cancellationToken))
    {
      logger.LogInformation("Could not remove POI for non-existent city with id {cityId}.", cityId);
      return NotFound();
    }

    var pointOfInterestEntity = await cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId, cancellationToken);
    if (pointOfInterestEntity == null)
    {
      return NotFound();
    }

    cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity, cancellationToken);
    await cityInfoRepository.SaveChangesAsync(cancellationToken);

    mailService.SendMail("Point of interest deleted",
      $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} has been deleted.");

    return NoContent();
  }

  [HttpPut("{pointOfInterestId}")]
  public async Task<IActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId, PointOfInterestForCreationDto pointOfInterest,
    CancellationToken cancellationToken = default)
  {
    if (!await cityInfoRepository.CityExistsAsync(cityId, cancellationToken))
    {
      logger.LogInformation("Could not update POI for non-existent city with id {cityId}.", cityId);
      return NotFound();
    }

    var pointOfInterestEntity = await cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId, cancellationToken);
    if (pointOfInterestEntity == null)
    {
      return NotFound();
    }

    mapper.Map(pointOfInterest, pointOfInterestEntity);
    await cityInfoRepository.SaveChangesAsync(cancellationToken);

    return NoContent();
  }

  [HttpPatch("{pointOfInterestId}")]
  public async Task<IActionResult> PartialUpdatePointOfInterest(int cityId, int pointOfInterestId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument,
    CancellationToken cancellationToken = default)
  {
    if (!await cityInfoRepository.CityExistsAsync(cityId, cancellationToken))
    {
      logger.LogInformation("Could not update POI for non-existent city with id {cityId}.", cityId);
      return NotFound();
    }

    var pointOfInterestEntity = await cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId, cancellationToken);
    if (pointOfInterestEntity == null)
    {
      return NotFound();
    }

    var pointOfInterestToPatch = mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

    patchDocument.ApplyTo(pointOfInterestToPatch, JsonPatchError =>
    {
      var key = JsonPatchError.AffectedObject.GetType().Name;
      ModelState.AddModelError(key, JsonPatchError.ErrorMessage);
    });

    if (!ModelState.IsValid || !TryValidateModel(pointOfInterestToPatch))
    {
      return BadRequest(ModelState);
    }

    mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);
    await cityInfoRepository.SaveChangesAsync(cancellationToken);

    return NoContent();
  }
}
