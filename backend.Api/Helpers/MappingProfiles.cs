using AutoMapper;
using backend.Api.DTOs;
using backend.Core.Entities;
using static backend.Api.Helpers.FacilityImageUrlResolver;

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
        CreateMap<Facility, FacilityDto>()
            .ForMember(f => f.ImageUrl, f => f.MapFrom<FacilityImageUrlResolver>());
        CreateMap<FacilityDto, Facility>();

        CreateMap<CourtDto, Court>().ReverseMap();
        CreateMap<Sport, SportDto>()
            .ForMember(d => d.ImageUrl, o => o.MapFrom<SportImageUrlResolver>());

        CreateMap<Match, MatchDto>()
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
    .ForMember(dest => dest.SportName, opt => opt.MapFrom(src => src.Sport.Name))
    .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Booking.Date))
    .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.Booking.StartTime))
    .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.Booking.EndTime))
    .ForMember(dest => dest.Players, opt => opt.MapFrom(src =>
        src.Players
           .Where(p => p.Status != ParticipationStatus.Kicked)
    ));

        CreateMap<MatchPlayer, MatchPlayerDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                src.User != null ? src.User.UserName : "User"))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<PlayerRating, PlayerRatingDto>()
            .ForMember(dest => dest.RaterUserName, opt => opt.MapFrom(src => 
                src.RaterUser != null ? src.RatedUser.UserName : "User"))
            .ForMember(dest => dest.RatedUserName, opt => opt.MapFrom(src => 
                src.RatedUser != null ? src.RatedUser.UserName : "User"));
        CreateMap<FriendRequest, FriendRequestDto>().ReverseMap();
    }
}

    

    
