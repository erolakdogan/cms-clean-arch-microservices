using FluentAssertions;
using Moq;
using UserService.Application.Common.Abstractions;
using UserService.Application.Users;
using UserService.Application.Users.Queries;
using UserService.Domain.Entities;

namespace UserService.Tests.Handlers
{
    public class GetUserByIdHandlerTests
    {
        private readonly Mock<IUserRepository> _repo = new();
        private readonly UsersMapper _mapper = new();

        [Fact]
        public async Task Should_Return_Dto_When_Found()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "u@cms.local",
                DisplayName = "U",
                Roles = new[] { "Admin" }
            };
            _repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var sut = new GetUserByIdHandler(_repo.Object, _mapper);
            var dto = await sut.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

            dto.Id.Should().Be(user.Id);
            dto.Email.Should().Be(user.Email);
        }
    }
}
