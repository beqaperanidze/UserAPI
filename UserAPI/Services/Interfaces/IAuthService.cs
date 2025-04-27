using UserAPI.Models.Domain;
using UserAPI.Models.DTOs;

namespace UserAPI.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerUserDto);
    Task<AuthResponseDto> LoginAsync(LoginUserDto loginUserDto);
    Task<bool> UserExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    string GenerateJwtToken(User user);
}