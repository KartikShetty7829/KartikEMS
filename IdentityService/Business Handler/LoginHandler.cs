using IdentityService.Data;
using IdentityService.Domain;
using IdentityService.Entities;
using IdentityService.Exceptions;
using IdentityService.Infrastructure;
using IdentityService.Interface;
using IdentityService.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityService.Business_Handler
{
    public class LoginHandler : IRequestHandler<LoginRequestDto, LoginResponseDto>
    {
        private readonly IGenericRepository<DbContext, User> _userRepo;
        private readonly IJwtService _jwtService;
        private readonly ILogger<LoginHandler> _logger;
        private readonly IPasswordHasherService _passwordHasher;

        public LoginHandler(
            IGenericRepository<DbContext, User> userRepo,
            IJwtService jwtService,
            ILogger<LoginHandler> logger,
            IPasswordHasherService passwordHasher)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResponseDto> Handle(LoginRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Login attempt for user {Username}", request.Username);

            // ✅ Load user with role
            var user = _userRepo.GetUsers()
                                .Include(u => u.Role)
                                .FirstOrDefault(u => u.Username == request.Username);

            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.Password))
            {
                _logger.LogWarning("Invalid login attempt for user {Username}", request.Username);
                throw new UnauthorizedException("Invalid username or password.");
            }

            // ✅ Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Inactive user {Username} attempted login", request.Username);
                throw new UnauthorizedException("Your account is inactive. you resigned from this company .");
            }

            // ✅ Check if user role is Employee
            if (user.Role?.Name != "Employee")
            {
                _logger.LogWarning("Unauthorized login attempt by non-employee user {Username}", request.Username);
                throw new UnauthorizedException("You are not authorized as employee.");
            }

            // ✅ Generate JWT token with role claim
            var token = _jwtService.GenerateJwtToken(user, user.Role.Name);

            _logger.LogInformation("Employee {Username} logged in successfully", user.Username);

            return new LoginResponseDto
            {
                Success = true,
                EmployeeNumber = user.EmployeeNumber,
                Message = "Employee login successful.",
                Token = token,
                Role = user.Role.Name,
                Username = user.Username,
                UserId = user.UserId
            };
        }
    }
}
