using IdentityService.Domain;
using IdentityService.Entities;
using IdentityService.Exceptions;
using IdentityService.Interface;
using IdentityService.Services;
using MediatR;
using Microsoft.Extensions.Logging;
namespace IdentityService.Business_Handler
{
    public class AdminLoginHandler : IRequestHandler<AdminLoginRequestDto, AdminLoginResponseDto>
    {
        private readonly IUserRepository _userRepo;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AdminLoginHandler> _logger;
        private readonly IPasswordHasherService _passwordHasher;

        public AdminLoginHandler(
            IUserRepository userRepo,
            IJwtService jwtService,
            ILogger<AdminLoginHandler> logger,
            IPasswordHasherService passwordHasher)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        public async Task<AdminLoginResponseDto> Handle(AdminLoginRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Admin login attempt for user {Username}", request.Username);

            // ✅ Use the dedicated repository method (fetch by username only)
            var user = await _userRepo.GetAdminUserByUsernameAsync(request.Username, cancellationToken);

            // ✅ Step 1: Check if user exists and verify password
            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.Password))
            {
                _logger.LogWarning("Invalid admin login attempt for {Username}", request.Username);
                throw new UnauthorizedException("Invalid username or password.");
            }

            // ✅ Step 2: Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Inactive admin {Username} attempted login", request.Username);
                throw new UnauthorizedException("Your admin account is inactive. you resigned from this Company.");
            }

            // ✅ Step 2: Role check is already handled in repository (only Admin returned)
            // But we can double-check for safety
            if (user.Role == null || !user.Role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Unauthorized admin login attempt by {Username} with role {Role}",
                    user.Username, user.Role?.Name ?? "None");

                throw new UnauthorizedException("Access denied. You are not authorized as Admin.");
            }

            // ✅ Step 3: Generate JWT token with role claim
            var token = _jwtService.GenerateJwtToken(user, user.Role.Name);
            _logger.LogInformation("Admin {Username} logged in successfully", user.Username);

            return new AdminLoginResponseDto
            {
                Success = true,
                Message = "Admin login successful.",
                Token = token,
                Role = user.Role.Name,
                Username = user.Username,
                AdminUserId = user.UserId,
                EmployeeNumber = user.EmployeeNumber,
            };
        }
    }
}
