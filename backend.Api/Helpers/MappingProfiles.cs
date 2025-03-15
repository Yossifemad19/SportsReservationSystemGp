using AutoMapper;
using backend.Api.DTOs;
using backend.Core.Entities;

namespace backend.Api.Helpers;

public class MappingProfiles: Profile
{
    public MappingProfiles()
    {
        CreateMap<RegisterDto, User>();
        CreateMap<OwnerRegisterDto, Owner>();
        // CreateMap<FacilityOwnerDTO, OwnerProfile>()
        //     // because I'm not able to map it direct
        //     .ForPath(dest => dest.UserCredential.Email, opt => opt.MapFrom(src => src.Email));

        CreateMap<AddressDto, Address>().ReverseMap();
        CreateMap<FacilityDto, Facility>().ReverseMap();

        CreateMap<CourtDto, Court>().ReverseMap();
        CreateMap<Sport, SportDto>();
    }
}

    

    
