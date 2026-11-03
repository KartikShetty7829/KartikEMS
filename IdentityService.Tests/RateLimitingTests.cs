using FluentAssertions;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection;
using System.Threading.RateLimiting;
using Xunit;

namespace IdentityService.Tests
{
    public class RateLimitingTests
    {
        [Fact]
        public void IdentityController_HasGeneralRateLimitingPolicyAttribute()
        {
            // Assert that IdentityController has the class-level general policy
            var controllerType = typeof(IdentityController);
            var attribute = controllerType.GetCustomAttribute<EnableRateLimitingAttribute>();

            attribute.Should().NotBeNull("IdentityController should have [EnableRateLimiting] applied at class level");
            attribute!.PolicyName.Should().Be("GeneralPolicy");
        }

        [Theory]
        [InlineData("Login")]
        [InlineData("AdminLogin")]
        [InlineData("Register")]
        [InlineData("ForgotPassword")]
        public void IdentityController_AuthEndpoints_HaveLoginRateLimitingPolicyAttribute(string actionMethodName)
        {
            // Assert that critical auth endpoints have the strict LoginPolicy (5 req/min)
            var controllerType = typeof(IdentityController);
            var method = controllerType.GetMethod(actionMethodName);

            method.Should().NotBeNull($"Method {actionMethodName} should exist on IdentityController");
            var attribute = method!.GetCustomAttribute<EnableRateLimitingAttribute>();

            attribute.Should().NotBeNull($"Method {actionMethodName} must be protected with [EnableRateLimiting]");
            attribute!.PolicyName.Should().Be("LoginPolicy");
        }

        [Fact]
        public void FixedWindowRateLimiter_EnforcesLimitAndRejectsExcessRequests()
        {
            // Educational demonstration of the underlying .NET 8 RateLimiter mechanics
            var options = new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            };

            using var limiter = new FixedWindowRateLimiter(options);

            // First 5 requests within the window must be permitted
            for (int i = 0; i < 5; i++)
            {
                using var lease = limiter.AttemptAcquire(1);
                lease.IsAcquired.Should().BeTrue($"Request #{i + 1} should be acquired within permit limit of 5");
            }

            // The 6th request exceeds the limit and must be rejected immediately (HTTP 429 scenario)
            using var excessLease = limiter.AttemptAcquire(1);
            excessLease.IsAcquired.Should().BeFalse("The 6th request within the 1-minute window must be rejected");
        }
    }
}

