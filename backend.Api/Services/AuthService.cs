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
    public async Task<string> Register(RegisterDto registerDto, UserRole userRole)
    {
        var user_profile = _mapper.Map<User>(registerDto);
        user_profile.UserRole = userRole;

        user_profile.PasswordHash = GetHashedPassword(registerDto.Password);

        // var result =await _userRepository.AddAsync(user_profile);
        _unitOfWork.Repository<User>().Add(user_profile);
        if (await _unitOfWork.CompleteAsync() > 0)
        {
            var token = _tokenService.GenerateToken(user_profile);
            return token;
        }
        return null;
    }

    public async Task<string> Login(LoginDto loginDto)
    {
        var user = await _unitOfWork.Repository<User>().FindAsync(x => x.Email == loginDto.Email);
        if (user != null && ValidatePassword(loginDto.Password, user.PasswordHash))
        {
            var token = _tokenService.GenerateToken(user);
            return token;
        }
        return null;
    }

    


    public async Task<string> OwnerRegister(OwnerRegisterDto ownerRegisterDto, UserRole userRole)
    {
        var owner = _mapper.Map<Owner>(ownerRegisterDto);
        owner.UserRole = userRole;
        owner.PasswordHash = GetHashedPassword(ownerRegisterDto.Password);

        owner.IsApproved = false;

        _unitOfWork.Repository<Owner>().Add(owner);
        if (await _unitOfWork.CompleteAsync() > 0)
        {
            return "Registration successful & Please wait for admin approval";
        }
        return null;
    }


    public async Task<string> OwnerLogin(OwnerLoginDto ownerLoginDto)
    {
        var owner = await _unitOfWork.Repository<Owner>().FindAsync(x => x.Email == ownerLoginDto.Email);

        if (!owner.IsApproved)
        {
            return ("Your account has not been approved yet");
            //throw new UnauthorizedAccessException
        }

        if (owner != null && ValidatePassword(ownerLoginDto.Password, owner.PasswordHash))
        {
            var token = _tokenService.GenerateToken(owner);
            return token;
        }

        return null; 
    }




    public static string  GetHashedPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool ValidatePassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}