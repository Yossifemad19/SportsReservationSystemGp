using AutoMapper;
using backend.Api.DTOs;
using backend.Api.Services.Interfaces;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Repository.Data;


namespace backend.Api.Services;

public class AuthService: IAuthService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;


    public AuthService(IMapper mapper,IUnitOfWork unitOfWork,ITokenService tokenService,IEmailService emailservice)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _emailService = emailservice;
    }
    public async Task<string?> Register(RegisterDto registerDto, UserRole userRole)
    {
        var user_profile = _mapper.Map<User>(registerDto);
        user_profile.UserRole = userRole;

        user_profile.PasswordHash = GetHashedPassword(registerDto.Password);

        _unitOfWork.Repository<User>().Add(user_profile);
        if (await _unitOfWork.CompleteAsync() > 0)
        {
            var token = _tokenService.GenerateToken(user_profile);
            return token;
        }
        return null;
    }

    public async Task<UserResponseDto?> Login(LoginDto loginDto)
    {
        var user = await _unitOfWork.Repository<User>().FindAsync(x => x.Email == loginDto.Email);
        if (user != null && ValidatePassword(loginDto.Password, user.PasswordHash))
        {
            var token = _tokenService.GenerateToken(user);
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.FirstName + " " + user.LastName,
                Email = user.Email,
                Token = token,
                Role = user.UserRole.ToString(),
                Message = "Logged in successfully"
            };
        }
        return null;
    }

    


public async Task<string?> OwnerRegister(OwnerRegisterDto ownerRegisterDto, UserRole userRole)
    {
        var owner = _mapper.Map<Owner>(ownerRegisterDto);
        owner.UserRole = userRole;
        owner.PasswordHash = GetHashedPassword(ownerRegisterDto.Password);

        owner.IsApproved = false;

        _unitOfWork.Repository<Owner>().Add(owner);
        if (await _unitOfWork.CompleteAsync() > 0)
        {
            return "Registration successfull & Please wait for admin approval";
        }
        return null;
    }


    public async Task<OwnerResponseDto?> OwnerLogin(OwnerLoginDto ownerLoginDto)
    {
        var owner = await _unitOfWork.Repository<Owner>().FindAsync(x => x.Email == ownerLoginDto.Email);

        if (owner == null)
        {
            return null;
        }

        if (!owner.IsApproved)
        {
            return new OwnerResponseDto { Message = "Your account has not been approved yet" };
        }

        if (ValidatePassword(ownerLoginDto.Password, owner.PasswordHash))
        {
            var token = _tokenService.GenerateToken(owner);
            return new OwnerResponseDto
            {
                Id = owner.Id,
                Name = owner.FirstName + " " + owner.LastName,
                Email = owner.Email,
                Token = token,
                Role = owner.UserRole.ToString(),
                IsApproved = owner.IsApproved.ToString(),
                Message = "Logged in successfully"
            };
        }

        return null;
    }

    public async Task<GetAllResponse?> GetUserById(int id)
    {
        var user = await _unitOfWork.Repository<User>()
                                    .GetByIdAsync(id);

        if (user == null)
        {
            return null;
        }

        return new GetAllResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
        };
    }

    public async Task<string?> ForgotPassword(string email)
    {
        var user = await _unitOfWork.Repository<User>().FindAsync(x => x.Email == email);
        if (user == null)
        {
            return null;
        }

        var resetToken = Guid.NewGuid().ToString();

        user.ResetToken = resetToken;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.CompleteAsync();

        var resetLink = $"http://localhost:5000/api/auth/reset-password?token={Uri.EscapeDataString(resetToken)}";
        await _emailService.SendEmailAsync(user.Email, "Password Reset", $"Click here to reset your password: {resetLink}");

        return $"Password reset token is: {resetToken}";
    }
    public async Task<string?> ResetPassword(PasswordDto passwordDto)
    {
        var user = await _unitOfWork.Repository<User>().FindAsync(x => x.ResetToken == passwordDto.Token);
        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            return null;
        }

        user.PasswordHash = GetHashedPassword(passwordDto.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.CompleteAsync();

        return "Password has been reset successfully.";
    }



    public async Task<bool> IsEmailExist(string email)
    {

        var userExists = await _unitOfWork.Repository<User>().FindAsync(x => x.Email == email) != null;

        var ownerExists = await _unitOfWork.Repository<Owner>().FindAsync(x => x.Email == email) != null;

        return userExists || ownerExists;
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