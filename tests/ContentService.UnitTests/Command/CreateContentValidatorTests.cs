using FluentAssertions;
using ContentService.Application.Contents.Commands;
using ContentService.Domain.Entities;

namespace ContentService.Tests.Validators
{
    public class CreateContentValidatorTests
    {
        [Fact]
        public async Task Valid_Model_Should_Pass()
        {
            var v = new CreateContentValidator();
            var r = await v.ValidateAsync(new CreateContentCommand("T", "B", Guid.NewGuid(), "t", ContentStatus.Draft));
            r.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Empty_Title_Should_Fail()
        {
            var v = new CreateContentValidator();
            var r = await v.ValidateAsync(new CreateContentCommand("", "B", Guid.NewGuid(), "t", ContentStatus.Draft));
            r.IsValid.Should().BeFalse();
        }
    }
}
