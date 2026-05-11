using ActivityTracker.Application.DTOs;
using ActivityTracker.Application.Interfaces;
using ActivityTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace ActivityTracker.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IActivityLogRepository activityLogRepository,
        IJwtTokenGenerator tokenGenerator,
        ICurrentUserService currentUserService,
        IPasswordHasher<AppUser> passwordHasher)
    {
        _userRepository = userRepository;
        _activityLogRepository = activityLogRepository;
        _tokenGenerator = tokenGenerator;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }

    public async Task RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var user = new AppUser
        {
            Username = request.Username,
            Role = request.Role
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        await _userRepository.AddAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null)
        {
            await LogActivityAsync(null, "FailedLogin", "Username not found.", cancellationToken);
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            await LogActivityAsync(user.Id, "FailedLogin", "Password mismatch.", cancellationToken);
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        await LogActivityAsync(user.Id, "Login", "User logged in.", cancellationToken);
        return _tokenGenerator.Generate(user);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await LogActivityAsync(_currentUserService.UserId, "Logout", "User logged out.", cancellationToken);
    }

    private Task LogActivityAsync(int? userId, string type, string description, CancellationToken cancellationToken)
    {
        return _activityLogRepository.AddAsync(new ActivityLog
        {
            UserId = userId,
            ActivityType = type,
            Description = description,
            Endpoint = string.Empty,
            Method = string.Empty,
            StatusCode = 200,
            IpAddress = _currentUserService.IpAddress,
            TimestampUtc = DateTime.UtcNow
        }, cancellationToken);
    }
}
