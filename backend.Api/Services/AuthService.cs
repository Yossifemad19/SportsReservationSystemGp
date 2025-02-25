using AutoMapper;
using backend.Api.DTOs;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Repository.Data;


namespace backend.Api.Services;

public class AuthService: IAuthService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    
    public AuthService(IMapper mapper,IUnitOfWork unitOfWork,ITokenService tokenService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }
    public async Task<string> Register(RegisterDto registerDto)
    {
        var user_profile = _mapper.Map<UserProfile>(registerDto);
        user_profile.UserCredential.PasswordHash=GetHashedPassword(registerDto.Password);
        // var result =await _userRepository.AddAsync(user_profile);
        _unitOfWork.Repository<UserProfile>().Add(user_profile);
        if (await _unitOfWork.CompleteAsync() > 0)
        {
            var token = _tokenService.GenerateToken(user_profile);
            return token;
        }
        return null;
    }

    public async Task<string> Login(LoginDto loginDto)
    {
        // not work yet   //need specification for include or another handling way
        var user=await _unitOfWork.Repository<UserProfile>().FindAsync(x=>x.UserCredential.Email  == loginDto.Email,x=>x.UserCredential);
        if (user != null && ValidatePassword(loginDto.Password, user.UserCredential.PasswordHash))
        {
            var token = _tokenService.GenerateToken(user);
            return token;
        }
        return null;  
    }

    public async Task<string> OwnerRegister(FacilityOwnerDTO facilityOwnerDTO)
    {
        var ownerProfile = _mapper.Map<OwnerProfile>(facilityOwnerDTO);

        // Ensure UserCredential is initialized
        ownerProfile.UserCredential = new UserCredential
        {
            Email = facilityOwnerDTO.Email,
            PasswordHash = GetHashedPassword(facilityOwnerDTO.Password)
        };

        // var result = await _ownerRepository.AddAsync(ownerProfile);
        _unitOfWork.Repository<OwnerProfile>().Add(ownerProfile);
        if (await _unitOfWork.CompleteAsync()> 0)
        {
            var token = _tokenService.GenerateToken(ownerProfile);
            return token;
        }

        return null;
    }

    public async Task<string> OwnerLogin(OwnerLoginDto ownerLoginDto)
    {
        // not work yet   //need specification for include or another handling way
        var owner = await _ownerRepository.FindAsync(x => x.UserCredential.Email == ownerLoginDto.Email, x => x.UserCredential);
        if (owner != null && ValidatePassword(ownerLoginDto.Password, owner.UserCredential.PasswordHash))
        {
            var token = _tokenService.GenerateToken(owner);
            return token;
        }
        return null;
    }


    private string GetHashedPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool ValidatePassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}