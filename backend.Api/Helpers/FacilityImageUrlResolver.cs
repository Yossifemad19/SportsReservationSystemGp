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
            return source.ImageUrl;
        }

        return null;
    }

    public class SportImageUrlResolver : IValueResolver<Sport, SportDto, string>
    {
        private readonly IConfiguration _configuration;

        public SportImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Sport source, SportDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.ImageUrl))
            {
                return  source.ImageUrl;
            }

            return null;
        }
    }

}