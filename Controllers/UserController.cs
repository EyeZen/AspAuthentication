﻿using AspAuthentication.Data;
using AspAuthentication.Data.Models;
using AspAuthentication.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AspAuthentication.Auth.Enums;

namespace AspAuthentication.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;

        public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, TokenService tokenService, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegistrationRequest request)
        {
            // TODO: What causes invalid ModelState?
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userManager.CreateAsync(
                new ApplicationUser { UserName = request.Username, Email = request.Email, Role = Role.User },
                request.Password
            );

            if (result.Succeeded)
            {
                request.Password = "";
                return CreatedAtAction(
                    nameof(Register),
                    new { email = request.Email, role = request.Role },
                    request.Password
                );
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthResponse>> Authentiacate([FromBody] AuthRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var managedUser = await _userManager.FindByEmailAsync(request.Email);
            if (managedUser == null)
            {
                return BadRequest("Bad Credentials");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password);
            if (!isPasswordValid)
            {
                return BadRequest("Bad Credentials");
            }

            var userInDb = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (userInDb is null)
            {
                return Unauthorized();
            }

            var accessToken = _tokenService.CreateToken(userInDb);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                Username = userInDb.UserName!,
                Email = userInDb.Email!,
                Token = accessToken,
            });
        }
    }
}
