using FluentAssertions;
using ContentService.Application.Contents.Commands;
using ContentService.Domain.Entities;

namespace ContentService.Tests.Validators
{
    public class UpdateContentValidatorTests
    {
        [Fact]
        public async Task Valid_Model_Should_Pass()
        {
            var v = new UpdateContentValidator();
            var r = await v.ValidateAsync(new UpdateContentCommand(Guid.NewGuid(), "T", "B", Guid.NewGuid(), "t", ContentStatus.Published));
            r.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Empty_Id_Should_Fail()
        {
            var v = new UpdateContentValidator();
            var r = await v.ValidateAsync(new UpdateContentCommand(Guid.Empty, "T", "B", Guid.NewGuid(), "t", ContentStatus.Published));
            r.IsValid.Should().BeFalse();
        }
    }
}
