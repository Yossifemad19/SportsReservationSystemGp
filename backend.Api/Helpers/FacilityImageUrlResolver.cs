using AutoMapper;
using backend.Api.DTOs;
using backend.Core.Entities;

namespace backend.Api.Helpers;

public class FacilityImageUrlResolver:IValueResolver<Facility,FacilityDto,string>
{
    private readonly IConfiguration _configuration;

    public FacilityImageUrlResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string Resolve(Facility source, FacilityDto destination, string destMember, ResolutionContext context)
    {
        if (!String.IsNullOrEmpty(source.ImageUrl))
        {
            return _configuration["ApiUrl"]+source.ImageUrl;
        }

        return null;
    }
}