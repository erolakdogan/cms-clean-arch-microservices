using FluentAssertions;
using Moq;
using UserService.Application.Common.Abstractions;
using UserService.Application.Users.Commands;
using UserService.Domain.Entities;

namespace UserService.Tests.Handlers
{
    public class UpdateUserHandlerTests
    {
        private readonly Mock<IUserRepository> _repo = new();
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IPasswordHasherService> _hasher = new();

        [Fact]
        public async Task Should_Update_DisplayName_And_Password_When_Provided()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "x@cms.local", PasswordHash = "OLD", DisplayName = "Old" };
            _repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _hasher.Setup(h => h.Hash("NewPass!")).Returns("NEW_HASH");

            var sut = new UpdateUserHandler(_repo.Object, _uow.Object, _hasher.Object);
            var cmd = new UpdateUserCommand(user.Id, "x@cms.local", "NewPass!", "New Name", new[] { "Admin" });

            await sut.Handle(cmd, CancellationToken.None);

            user.DisplayName.Should().Be("New Name");
            user.PasswordHash.Should().Be("NEW_HASH");
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
