using FluentAssertions;
using Moq;
using Shared.Web.AsyncQuerying;
using Shared.Web.Fakes;
using UserService.Application.Common.Abstractions;
using UserService.Application.Common.Models;
using UserService.Application.Users;
using UserService.Application.Users.Queries;

namespace UserService.Tests.Handlers
{
    public class ListUsersHandlerTests
    {
        private readonly Mock<IUserRepository> _repo = new();
        private readonly UsersMapper _mapper = new();

        [Fact]
        public async Task Should_Return_Paged()
        {
            var items = FakesData.ManyUsers(25).ToList();
            _repo.Setup(r => r.Query()).Returns(items.AsAsyncQueryable());

            var sut = new ListUsersHandler(_repo.Object, _mapper);
            PagedResult<UserDto> result = await sut.Handle(new ListUsersQuery(2, 10, null), CancellationToken.None);

            result.Items.Should().HaveCount(10);
            result.Page.Should().Be(2);
            result.TotalItems.Should().Be(25);
        }
    }
}
