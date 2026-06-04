namespace CityInfo.Api.Models;

public class PointOfInterestCreationResult
{
  public bool Success { get; set; }
  public string? ErrorMessage { get; set; }
  public PointOfInterestDto? PointOfInterest { get; set; }

  public static PointOfInterestCreationResult Successful(PointOfInterestDto pointOfInterest) =>
    new()
    { Success = true, PointOfInterest = pointOfInterest };

  public static PointOfInterestCreationResult Failed(string errorMessage) =>
    new()
    { Success = false, ErrorMessage = errorMessage };
}
