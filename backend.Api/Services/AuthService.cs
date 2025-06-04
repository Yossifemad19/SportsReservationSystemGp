using AutoMapper;
using backend.Api.DTOs;
using backend.Api.Services.Interfaces;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace backend.Api.Services;

public class AuthService : IAuthService
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

    public async Task<string> Register(RegisterDto registerDto, string roleName)
    {
        // Get or create the customer role
        var userRole = await GetOrCreateUserRole(roleName);

        var user = _mapper.Map<User>(registerDto);
        user.UserRoleId = userRole.Id;
        user.UserName = registerDto.Email.Split('@')[0];
        user.PasswordHash = GetHashedPassword(registerDto.Password);

        _unitOfWork.Repository<User>().Add(user);
        if (await _unitOfWork.Complete() > 0)
        {
            // Create default player profile for the new user
            try
            {
                // Create player profile for customer/player role
                if (roleName == "Customer")
                {
                    var playerProfile = new PlayerProfile
                    {
                        UserId = user.Id,
                        SkillLevel = 5, // Default skill level
                        PreferredPlayingStyle = "Balanced",
                        PreferredSports = new List<string>(),
                        PreferredPlayingTimes = new List<string>(),
                        PrefersCompetitivePlay = true,
                        PrefersCasualPlay = true,
                        PreferredTeamSize = 2,
                        WeeklyAvailability = new Dictionary<DayOfWeek, List<TimeSpan>>(),
                        SportSpecificSkills = new Dictionary<string, int>(),
                        FrequentPartners = new List<string>(),
                        BlockedPlayers = new List<string>(),
                        MatchesPlayed = 0,
                        MatchesWon = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                    
                    _unitOfWork.Repository<PlayerProfile>().Add(playerProfile);
                    await _unitOfWork.Complete();
                }
            }
            catch (Exception ex)
            {
                // Log error but continue - user is already created
                // We can create profile later if this fails
            }

            var token = _tokenService.GenerateToken(user);
            return token;
        }
        return null;
    }

    public async Task<UserResponseDto?> Login(LoginDto loginDto)
    {
        var user = await _unitOfWork.Repository<User>().FindAsync(x => x.Email == loginDto.Email);
        if (user != null && ValidatePassword(loginDto.Password, user.PasswordHash))
        {
            // Load the UserRole
            var userRole = await _unitOfWork.Repository<UserRole>().GetByIdAsync(user.UserRoleId);
            
            var token = _tokenService.GenerateToken(user);
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.FirstName + " " + user.LastName,
                Email = user.Email,
                Token = token,
                Role = userRole?.RoleName ?? "Unknown",
                Message = "Logged in successfully"
            };
        }
        return null;
    }

    public async Task<string> OwnerRegister(OwnerRegisterDto ownerRegisterDto, string roleName)
    {
        // Get or create the owner role
        var userRole = await GetOrCreateUserRole(roleName);

        var owner = _mapper.Map<Owner>(ownerRegisterDto);
        owner.UserRoleId = userRole.Id;
        owner.PasswordHash = GetHashedPassword(ownerRegisterDto.Password);
        owner.IsApproved = false;

        _unitOfWork.Repository<Owner>().Add(owner);
        if (await _unitOfWork.Complete() > 0)
        {
            return "Registration successfull & Please wait for admin approval";
        }
        return null;
    }

    public async Task<OwnerResponseDto> OwnerLogin(OwnerLoginDto ownerLoginDto)
    {
        var owner = await _unitOfWork.Repository<Owner>().FindAsync(x => x.Email == ownerLoginDto.Email);

        if (owner == null)
        {
            return null;
        }

        if (ValidatePassword(ownerLoginDto.Password, owner.PasswordHash))
        {
            // Load the UserRole
            var userRole = await _unitOfWork.Repository<UserRole>().GetByIdAsync(owner.UserRoleId);
            
            var token = _tokenService.GenerateToken(owner);
            return new OwnerResponseDto
            {
                Id = owner.Id,
                Name = owner.FirstName + " " + owner.LastName,
                Email = owner.Email,
                Token = token,
                Role = userRole?.RoleName ?? "Unknown",
                IsApproved = owner.IsApproved.ToString(),
                Message = "Logged in successfully"
            };
        }

        return null;
    }

    public async Task<GetAllResponse> GetUserById(int id)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetByIdAsync(id);

        if (user == null)
        {
            throw new Exception($"User with id {id} not found.");
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

    private async Task<UserRole> GetOrCreateUserRole(string roleName)
    {
        var roleSpec = new UserRoleSpecification(roleName);
        var userRole = await _unitOfWork.Repository<UserRole>().GetFirstOrDefaultAsync(roleSpec);
        
        if (userRole == null)
        {
            userRole = new UserRole
            {
                RoleName = roleName,
                Description = $"{roleName} role"
            };
            
            _unitOfWork.Repository<UserRole>().Add(userRole);
            await _unitOfWork.Complete();
        }
        
        return userRole;
    }

    public async Task<UserProfileDto> UpdateUserProfile(int userId, UserProfileDto userProfile)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            throw new Exception($"User with id {userId} not found.");
        }

        user.FirstName = userProfile.FirstName;
        user.LastName = userProfile.LastName;
        user.UserName = userProfile.UserName ?? user.Email.Split('@')[0];
        user.Email = userProfile.Email;
        user.PhoneNumber = userProfile.PhoneNumber;
        _unitOfWork.Repository<User>().Update(user);
        if (await _unitOfWork.Complete() > 0)
        {
            return new UserProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber

            };
        }

        return null;
    }

    public async Task<bool> DeleteUser(int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            throw new Exception($"User with id {userId} not found.");
        }

        _unitOfWork.Repository<User>().Remove(user);
        return await _unitOfWork.Complete() > 0;
    }


    public async Task<string> ForgotPassword(string email)
    {
        var user = await _unitOfWork.Repository<User>().FindAsync(x => x.Email == email);
        if (user == null)
        {
            return "User with this email does not exist.";
        }


        var resetToken = Guid.NewGuid().ToString();


        user.ResetToken = resetToken;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.Complete();


        var resetLink = $"http://localhost:5000/api/auth/reset-password?token={Uri.EscapeDataString(resetToken)}";
        await _emailService.SendEmailAsync(user.Email, "Password Reset", $"Click here to reset your password: {resetLink}");

        return $"Password reset token is: {resetToken}";

    }
    public async Task<string> ResetPassword(PasswordDto passwordDto)
    {
        var user = await _unitOfWork.Repository<User>().FindAsync(x => x.ResetToken == passwordDto.Token);
        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            return "Invalid or expired reset token.";
        }

        
        user.PasswordHash = GetHashedPassword(passwordDto.NewPassword);
        user.ResetToken = null; 
        user.ResetTokenExpiry = null;
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.Complete();

        return "Password has been reset successfully.";
    }



    public async Task<bool> IsEmailExist(string email)
    {

        var userExists = await _unitOfWork.Repository<User>().FindAsync(x => x.Email == email) != null;

        var ownerExists = await _unitOfWork.Repository<Owner>().FindAsync(x => x.Email == email) != null;

        return userExists || ownerExists;
    }

    public static string GetHashedPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool ValidatePassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}