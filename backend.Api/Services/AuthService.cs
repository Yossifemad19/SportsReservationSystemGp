using AutoMapper;
using backend.Api.DTOs;
using backend.Api.Services;
using backend.Api.Services.Interfaces;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;

public class AuthService : IAuthService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthService(IMapper mapper, IUnitOfWork unitOfWork, ITokenService tokenService, IEmailService emailService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<ServiceResult<string>> Register(RegisterDto registerDto, string roleName)
    {
        if (await IsEmailExist(registerDto.Email))
            return ServiceResult<string>.Fail("Email already in use.");

        var userRole = await GetOrCreateUserRole(roleName);

        var user = _mapper.Map<User>(registerDto);
        user.UserRoleId = userRole.Id;
        user.UserName = registerDto.Email.Split('@')[0];
        user.PasswordHash = GetHashedPassword(registerDto.Password);

        _unitOfWork.Repository<User>().Add(user);

        if (await _unitOfWork.Complete() > 0)
        {
            try
            {
                if (roleName == "Customer")
                {
                    var playerProfile = new PlayerProfile
                    {
                        UserId = user.Id,
                        SkillLevel = 5,
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
            catch
            {
                // Log error (optional)
            }

            var token = _tokenService.GenerateToken(user);
            return ServiceResult<string>.Ok(token, "Registration successful");
        }

        return ServiceResult<string>.Fail("Registration failed.");
    }

    public async Task<ServiceResult<UserResponseDto>> Login(LoginDto loginDto)
    {
        var user = await _unitOfWork.Repository<User>().FindAsync(x => x.Email == loginDto.Email);
        if (user == null || !ValidatePassword(loginDto.Password, user.PasswordHash))
            return ServiceResult<UserResponseDto>.Fail("Invalid email or password.");

        var userRole = await _unitOfWork.Repository<UserRole>().GetByIdAsync(user.UserRoleId);

        var dto = new UserResponseDto
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            Token = _tokenService.GenerateToken(user),
            Role = userRole?.RoleName ?? "Unknown",
            Message = "Logged in successfully"
        };

        return ServiceResult<UserResponseDto>.Ok(dto);
    }

    public async Task<ServiceResult<string>> OwnerRegister(OwnerRegisterDto ownerRegisterDto, string roleName)
    {
        if (await IsEmailExist(ownerRegisterDto.Email))
            return ServiceResult<string>.Fail("Email already in use.");

        var userRole = await GetOrCreateUserRole(roleName);

        var owner = _mapper.Map<Owner>(ownerRegisterDto);
        owner.UserRoleId = userRole.Id;
        owner.PasswordHash = GetHashedPassword(ownerRegisterDto.Password);
        owner.IsApproved = false;

        _unitOfWork.Repository<Owner>().Add(owner);
        if (await _unitOfWork.Complete() > 0)
        {
            return ServiceResult<string>.Ok("Registration successful. Please wait for admin approval.");
        }

        return ServiceResult<string>.Fail("Owner registration failed.");
    }

    public async Task<ServiceResult<OwnerResponseDto>> OwnerLogin(OwnerLoginDto ownerLoginDto)
    {
        var owner = await _unitOfWork.Repository<Owner>().FindAsync(x => x.Email == ownerLoginDto.Email);

        if (owner == null || !ValidatePassword(ownerLoginDto.Password, owner.PasswordHash))
            return ServiceResult<OwnerResponseDto>.Fail("Invalid email or password.");

        var userRole = await _unitOfWork.Repository<UserRole>().GetByIdAsync(owner.UserRoleId);

        var dto = new OwnerResponseDto
        {
            Id = owner.Id,
            Name = $"{owner.FirstName} {owner.LastName}",
            Email = owner.Email,
            Token = _tokenService.GenerateToken(owner),
            Role = userRole?.RoleName ?? "Unknown",
            IsApproved = owner.IsApproved.ToString(),
            Message = "Logged in successfully"
        };

        return ServiceResult<OwnerResponseDto>.Ok(dto);
    }

    public async Task<ServiceResult<GetAllResponse>> GetUserById(int id)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);

        if (user == null)
            return ServiceResult<GetAllResponse>.Fail($"User with id {id} not found.");

        var response = new GetAllResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        return ServiceResult<GetAllResponse>.Ok(response);
    }

    public async Task<ServiceResult<UserProfileDto>> UpdateUserProfile(int userId, UserProfileDto userProfile)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
            return ServiceResult<UserProfileDto>.Fail($"User with id {userId} not found.");

        user.FirstName = userProfile.FirstName;
        user.LastName = userProfile.LastName;
        user.UserName = userProfile.UserName ?? user.Email.Split('@')[0];
        user.Email = userProfile.Email;
        user.PhoneNumber = userProfile.PhoneNumber;

        _unitOfWork.Repository<User>().Update(user);
        if (await _unitOfWork.Complete() > 0)
        {
            return ServiceResult<UserProfileDto>.Ok(userProfile, "Profile updated successfully.");
        }

        return ServiceResult<UserProfileDto>.Fail("Failed to update user profile.");
    }

    public async Task<ServiceResult<bool>> DeleteUser(int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
            return ServiceResult<bool>.Fail($"User with id {userId} not found.");

        _unitOfWork.Repository<User>().Remove(user);
        var result = await _unitOfWork.Complete() > 0;

        return result
            ? ServiceResult<bool>.Ok(true, "User deleted successfully.")
            : ServiceResult<bool>.Fail("Failed to delete user.");
    }

    public async Task<ServiceResult<string>> ForgotPassword(string email)
    {
        var user = await _unitOfWork.Repository<User>().FindAsync(x => x.Email == email);
        if (user == null)
            return ServiceResult<string>.Fail("User with this email does not exist.");

        var resetToken = Guid.NewGuid().ToString();
        user.ResetToken = resetToken;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.Complete();

        var resetLink = $"http://localhost:5000/api/auth/reset-password?token={Uri.EscapeDataString(resetToken)}";
        await _emailService.SendEmailAsync(user.Email, "Password Reset", $"Click here to reset your password: {resetLink}");

        return ServiceResult<string>.Ok(resetToken, "Reset token generated.");
    }

    public async Task<ServiceResult<string>> ResetPassword(PasswordDto passwordDto)
    {
        var user = await _unitOfWork.Repository<User>().FindAsync(x => x.ResetToken == passwordDto.Token);
        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            return ServiceResult<string>.Fail("Invalid or expired reset token.");

        user.PasswordHash = GetHashedPassword(passwordDto.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.Complete();

        return ServiceResult<string>.Ok("Password has been reset successfully.");
    }

    public async Task<bool> IsEmailExist(string email)
    {   
        var userExists = await _unitOfWork.Repository<User>().FindAsync(x => x.Email == email) != null;
        var ownerExists = await _unitOfWork.Repository<Owner>().FindAsync(x => x.Email == email) != null;
        return userExists || ownerExists;
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

    public static string GetHashedPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool ValidatePassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
