using FluentAssertions;
using Moq;
using UserService.Application.Common.Abstractions;
using UserService.Application.Users.Commands;
using UserService.Domain.Entities;
using Xunit;

namespace UserService.Tests.Handlers
{
    public class CreateUserHandlerTests
    {
        private readonly Mock<IUserRepository> _repo = new();
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IPasswordHasherService> _hasher = new();

        [Fact]
        public async Task Should_Create_And_Return_Id()
        {
            var cmd = new CreateUserCommand("x@cms.local", "P@ss1!", "X", new[] { "Admin" });
            _hasher.Setup(h => h.Hash(cmd.Password!)).Returns("HASH");

            var sut = new CreateUserHandler(_repo.Object, _uow.Object, _hasher.Object);

            var id = await sut.Handle(cmd, CancellationToken.None);

            id.Should().NotBeEmpty();
            _repo.Verify(r => r.AddAsync(It.Is<User>(u =>
                u.Email == cmd.Email &&
                u.DisplayName == cmd.DisplayName &&
                u.PasswordHash == "HASH"), It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
