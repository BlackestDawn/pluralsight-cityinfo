using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CityInfo.Api.Models;

public record class CityDto
{
  public int Id { get; set; }
  [Required]
  [MaxLength(50)]
  public string Name { get; set; } = string.Empty;
  [MaxLength(200)]
  public string? Description { get; set; }
  public int NumberOfPointsOfInterest => PointsOfInterest.Count;
  public ICollection<PointOfInterestDto> PointsOfInterest { get; set; } = [];
}
