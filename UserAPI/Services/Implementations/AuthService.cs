using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserAPI.Auth;
using UserAPI.Data;
using UserAPI.Models.Domain;
using UserAPI.Models.DTOs;
using UserAPI.Services.Interfaces;

namespace UserAPI.Services.Implementations;

public class AuthService(ApplicationDbContext context, JwtSettings jwtSettings) : IAuthService

{
    private readonly ApplicationDbContext _context = context;
    private readonly JwtSettings _jwtSettings = jwtSettings;

    public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerUserDto)
    {
        if (await UserExistsAsync(registerUserDto.Username))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Username already exists"
            };
        }

        if (await EmailExistsAsync(registerUserDto.Email))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Email already exists"
            };
        }

        CreatePasswordHash(registerUserDto.Password, out byte[] passwordHash, out byte[] passwordSalt);


        var user = new User
        {
            Username = registerUserDto.Username,
            Email = registerUserDto.Email,
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Role = UserRole.USER
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return new AuthResponseDto()
        {
            Success = true,
            Token = token,
            Message = "User registered successfully",
            User = MapUserToDto(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginUserDto loginUserDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == loginUserDto.Username);
        if (user == null)
        {
            return new AuthResponseDto()
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        if (!VerifyPasswordHash(loginUserDto.Password, user.PasswordHash, user.PasswordSalt))
        {
            return new AuthResponseDto()
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        user.LastLoginAt = DateTime.Now;
        await _context.SaveChangesAsync();
        var token = GenerateJwtToken(user);
        return new AuthResponseDto
        {
            Success = true,
            Token = token,
            Message = "Login successful",
            User = MapUserToDto(user)
        };
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new HMACSHA512(storedSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != storedHash[i])
            {
                return false;
            }
        }

        return true;
    }

    private static UserDto MapUserToDto(User user)
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