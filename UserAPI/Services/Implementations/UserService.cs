using Microsoft.EntityFrameworkCore;
using UserAPI.Data;
using UserAPI.Models.Domain;
using UserAPI.Models.DTOs;
using UserAPI.Services.Interfaces;

namespace UserAPI.Services.Implementations;

public class UserService(ApplicationDbContext context) : IUserService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<UserDto?>> GetAllUsersAsync()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(MapUserToDto);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return user != null ? MapUserToDto(user) : null;
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
        return user != null ? MapUserToDto(user) : null;
    }

    public async Task<bool> UpdateUserAsync(int id, UserDto userDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;

        if (user.Email != userDto.Email)
        {
            var emailExists = await _context.Users.AnyAsync(u => u.Email == userDto.Email && u.Id != id);
            if (emailExists)
            {
                return false;
            }

            user.Email = userDto.Email;
        }

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangeUserRoleAsync(int id, UserRole role)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        user.Role = role;
        await _context.SaveChangesAsync();
        return true;
    }

    private static UserDto? MapUserToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            Role = user.Role
        };
    }
}