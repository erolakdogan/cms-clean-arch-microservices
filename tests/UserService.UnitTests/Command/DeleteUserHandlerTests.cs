using Moq;
using UserService.Application.Common.Abstractions;
using UserService.Application.Users.Commands;
using UserService.Domain.Entities;

namespace UserService.Tests.Handlers
{
    public class DeleteUserHandlerTests
    {
        private readonly Mock<IUserRepository> _repo = new();
        private readonly Mock<IUnitOfWork> _uow = new();

        [Fact]
        public async Task Should_Delete_When_Exists()
        {
            var user = new User { Id = Guid.NewGuid() };
            _repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var sut = new DeleteUserHandler(_repo.Object, _uow.Object);
            await sut.Handle(new DeleteUserCommand(user.Id), CancellationToken.None);

            _repo.Verify(r => r.Remove(user), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
