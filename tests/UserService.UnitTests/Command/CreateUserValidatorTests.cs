using FluentAssertions;
using UserService.Application.Users.Commands;
namespace UserService.Tests.Validators
{
    public class CreateUserValidatorTests
    {
        [Fact]
        public async Task Valid_Model_Should_Pass()
        {
            var v = new CreateUserValidator();
            var r = await v.ValidateAsync(new CreateUserCommand("x@cms.local", "Pass!123", "X", new[] { "Admin" }));
            r.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Empty_Email_Should_Fail()
        {
            var v = new CreateUserValidator();
            var r = await v.ValidateAsync(new CreateUserCommand("", "Pass!123", "X", new[] { "Admin" }));
            r.IsValid.Should().BeFalse();
        }
    }
}
