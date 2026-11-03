using IdentityService.Data;
using IdentityService.Domain;
using IdentityService.Entities;
using IdentityService.Exceptions;
using IdentityService.Infrastructure;
using IdentityService.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityService.Business_Handler
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserRequestDto, RegisterUserResponseDto>
    {
        private readonly IGenericRepository<DbContext, User> _userRepo;
        private readonly IGenericRepository<DbContext, Role> _roleRepo;
        private readonly ILogger<RegisterUserHandler> _logger;
        private readonly IUnitOfWork<IdentityDbContext> _unitOfWork;
        private readonly IPasswordHasherService _passwordHasher;

        public RegisterUserHandler(
            IGenericRepository<DbContext, User> userRepo,
            IGenericRepository<DbContext, Role> roleRepo,
            ILogger<RegisterUserHandler> logger,
            IUnitOfWork<IdentityDbContext> unitOfWork,
            IPasswordHasherService passwordHasher)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<RegisterUserResponseDto> Handle(RegisterUserRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting registration for user {Username}", request.Username);

            // ✅ 1. Check if username already exists
            var existingUsers = _userRepo.GetUsers()
                                         .Where(u => u.Username == request.Username)
                                         .ToList();

            if (existingUsers.Any())
            {
                throw new BadRequestException($"User '{request.Username}' already exists.");
            }

            // ✅ 2. Check if EmployeeNumber already has a user account
            var existingEmployeeUser = _userRepo.GetUsers()
                                                .Where(u => u.EmployeeNumber == request.EmployeeNumber)
                                                .FirstOrDefault();

            if (existingEmployeeUser != null)
            {
                throw new BadRequestException($"EmployeeNumber '{request.EmployeeNumber}' already has a user account.");
            }

            // ✅ 3. Validate role
            var role = _roleRepo.GetRoles()
                                .FirstOrDefault(r => r.Name == request.Role);

            if (role == null)
            {
                throw new NotFoundException($"Role '{request.Role}' not found.");
            }

            // ✅ 4. Create user WITH EmployeeNumber and Hashed Password
            var user = new User
            {
                Username = request.Username,
                Password = _passwordHasher.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                RoleId = role.RoleId,
                EmployeeNumber = request.EmployeeNumber
            };

            await _userRepo.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "User {Username} created with UserId {UserId}, Role {Role}, EmployeeNumber {EmployeeNumber}",
                user.Username, user.UserId, role.Name, user.EmployeeNumber
            );

            // ✅ 5. Build response
            return new RegisterUserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                EmployeeNumber = user.EmployeeNumber,
                CreatedAt = user.CreatedAt,
                Role = role.Name,
                Success = true,
                Message = $"User '{user.Username}' registered successfully with role '{role.Name}'."
            };
        }
    }
}
