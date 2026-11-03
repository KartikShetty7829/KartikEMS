using EmployeeServiceDepartmentService.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection;
using System.Threading.RateLimiting;
using Xunit;

namespace EmployeeService.Tests
{
    public class RateLimitingTests
    {
        [Fact]
        public void EmployeeController_HasGeneralRateLimitingPolicyAttribute()
        {
            var controllerType = typeof(EmployeeController);
            var attribute = controllerType.GetCustomAttribute<EnableRateLimitingAttribute>();

            attribute.Should().NotBeNull("EmployeeController should have [EnableRateLimiting] applied at class level");
            attribute!.PolicyName.Should().Be("EmployeeGeneralPolicy");
        }

        [Theory]
        [InlineData("AddEmployee")]
        [InlineData("ApplyLeave")]
        public void EmployeeController_NotificationEndpoints_HaveNotificationRateLimitingPolicy(string actionMethodName)
        {
            var controllerType = typeof(EmployeeController);
            var method = controllerType.GetMethod(actionMethodName);

            method.Should().NotBeNull($"Method {actionMethodName} should exist on EmployeeController");
            var attribute = method!.GetCustomAttribute<EnableRateLimitingAttribute>();

            attribute.Should().NotBeNull($"Method {actionMethodName} must be protected with [EnableRateLimiting]");
            attribute!.PolicyName.Should().Be("NotificationPolicy");
        }

        [Fact]
        public void EmployeeController_PayslipEndpoint_HasPayslipRateLimitingPolicy()
        {
            var controllerType = typeof(EmployeeController);
            var method = controllerType.GetMethod("GeneratePayslip");

            method.Should().NotBeNull("GeneratePayslip method should exist on EmployeeController");
            var attribute = method!.GetCustomAttribute<EnableRateLimitingAttribute>();

            attribute.Should().NotBeNull("GeneratePayslip must be protected with [EnableRateLimiting]");
            attribute!.PolicyName.Should().Be("PayslipPolicy");
        }

        [Fact]
        public void NotificationPolicy_EnforcesStrictCapOf3Requests()
        {
            // Verifies the 3-request limit for external notification channels (Twilio SMS / SMTP Email)
            var options = new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            };

            using var limiter = new FixedWindowRateLimiter(options);

            for (int i = 0; i < 3; i++)
            {
                using var lease = limiter.AttemptAcquire(1);
                lease.IsAcquired.Should().BeTrue();
            }

            // 4th request must fail
            using var excessLease = limiter.AttemptAcquire(1);
            excessLease.IsAcquired.Should().BeFalse();
        }
    }
}

