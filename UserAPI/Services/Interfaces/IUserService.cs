using UserAPI.Models.Domain;
using UserAPI.Models.DTOs;

namespace UserAPI.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto?>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<bool> UpdateUserAsync(int id, UserDto userDto);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> ChangeUserRoleAsync(int id, UserRole role);
}