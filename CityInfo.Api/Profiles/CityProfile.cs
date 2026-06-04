using AutoMapper;

namespace CityInfo.Api.Profiles;

public class CityProfile : Profile
{
  public CityProfile()
  {
    CreateMap<Entities.City, Models.CityWithoutPointsOfIntrestDto>();
    CreateMap<Entities.City, Models.CityDto>();
  }
}
