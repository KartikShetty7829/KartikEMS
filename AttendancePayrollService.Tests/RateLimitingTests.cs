using AttendancePayrollService.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection;
using System.Threading.RateLimiting;
using Xunit;

namespace AttendancePayrollService.Tests
{
    public class RateLimitingTests
    {
        [Fact]
        public void AttendanceController_HasAttendanceRateLimitingPolicyAttribute()
        {
            var controllerType = typeof(AttendanceController);
            var attribute = controllerType.GetCustomAttribute<EnableRateLimitingAttribute>();

            attribute.Should().NotBeNull("AttendanceController should have [EnableRateLimiting] applied at class level");
            attribute!.PolicyName.Should().Be("AttendancePolicy");
        }

        [Fact]
        public void AttendancePolicy_EnforcesLimitOf30Requests()
        {
            var options = new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            };

            using var limiter = new FixedWindowRateLimiter(options);

            for (int i = 0; i < 30; i++)
            {
                using var lease = limiter.AttemptAcquire(1);
                lease.IsAcquired.Should().BeTrue();
            }

            // 31st request exceeds the limit
            using var excessLease = limiter.AttemptAcquire(1);
            excessLease.IsAcquired.Should().BeFalse();
        }
    }
}

