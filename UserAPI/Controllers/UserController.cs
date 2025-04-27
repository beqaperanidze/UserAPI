using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAPI.Models.Domain;
using UserAPI.Models.DTOs;
using UserAPI.Services.Interfaces;

namespace UserAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize] 
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var user = await userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")] 
        public async Task<IActionResult> UpdateUser(int id, UserDto userDto)
        {
            var result = await userService.UpdateUserAsync(id, userDto);
            if (!result)
            {
                return BadRequest("Update failed.  Possible reasons: User not found, or email already exists.");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")] 
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "ADMIN")] 
        public async Task<IActionResult> ChangeUserRole(int id, UserRole role)
        {
            var result = await userService.ChangeUserRoleAsync(id, role);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}

