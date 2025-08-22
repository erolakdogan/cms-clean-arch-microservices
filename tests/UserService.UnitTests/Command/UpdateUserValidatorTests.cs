using FluentAssertions;
using UserService.Application.Users.Command.Update;
using UserService.Application.Users.Commands;

namespace UserService.Tests.Validators
{
    public class UpdateUserValidatorTests
    {
        [Fact]
        public async Task Valid_Model_Should_Pass()
        {
            var v = new UpdateUserValidator();
            var r = await v.ValidateAsync(new UpdateUserCommand(Guid.NewGuid(), "x@cms.local", null, "X", new[] { "User" }));
            r.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Empty_Id_Should_Fail()
        {
            var v = new UpdateUserValidator();
            var r = await v.ValidateAsync(new UpdateUserCommand(Guid.Empty, "x@cms.local", null, "X", new[] { "User" }));
            r.IsValid.Should().BeFalse();
        }
    }
}
