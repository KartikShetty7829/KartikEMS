using IdentityService.Data;
using IdentityService.Domain;
using IdentityService.Entities;
using IdentityService.Exceptions;
using IdentityService.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityService.Business_Handler
{
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordRequestDto, ForgotPasswordResponseDto>
    {
        private readonly IGenericRepository<DbContext, User> _userRepo;
        private readonly ILogger<ForgotPasswordHandler> _logger;

        public ForgotPasswordHandler(
            IGenericRepository<DbContext, User> userRepo,
            ILogger<ForgotPasswordHandler> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<ForgotPasswordResponseDto> Handle(ForgotPasswordRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Password reset attempt for user {Username}", request.Username);

            // ✅ Find user by username
            var user = _userRepo.GetUsers()
                                .FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
            {
                _logger.LogWarning("Password reset failed. User not found: {Username}", request.Username);
                throw new NotFoundException("User not found.");
            }

            // ✅ Update password
            user.Password = request.NewPassword;

            await _userRepo.UpdateAsync(user);

            _logger.LogInformation("Password reset successful for user {Username}", request.Username);

            return new ForgotPasswordResponseDto
            {
                Success = true,
                Message = "Password reset successfully."
            };
        }
    }
}
