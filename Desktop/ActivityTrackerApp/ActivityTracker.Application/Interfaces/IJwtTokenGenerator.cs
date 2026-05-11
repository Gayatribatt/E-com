using ActivityTracker.Application.DTOs;
using ActivityTracker.Domain.Models;

namespace ActivityTracker.Application.Interfaces;

public interface IJwtTokenGenerator
{
    AuthResponseDto Generate(AppUser user);
}
