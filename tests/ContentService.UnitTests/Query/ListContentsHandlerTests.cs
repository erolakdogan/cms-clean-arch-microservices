using ContentService.Application.Common.Abstractions;
using ContentService.Application.Common.Models;
using ContentService.Application.Contents;
using ContentService.Application.Contents.Queries;
using ContentService.Application.UsersExternal;
using FluentAssertions;
using Moq;
using Shared.Web.AsyncQuerying;
using Shared.Web.Fakes;

namespace ContentService.Tests.Handlers
{
    public class ListContentsHandlerTests
    {
        private readonly Mock<IContentRepository> _repo = new();
        private readonly ContentMapper _mapper = new();
        private readonly Mock<IUsersClient> _users = new();

        [Fact]
        public async Task Should_Return_Paged_And_Fill_AuthorName()
        {
            var items = FakesData.ManyContents(15).ToList();
            _repo.Setup(r => r.Query()).Returns(items.AsAsyncQueryable());

            _users.Setup(u => u.GetBriefAsync(It.IsAny<System.Guid>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((System.Guid id, CancellationToken _) =>
                        new UserBriefDto { Id = id, DisplayName = "Author X", Email = "x@cms.local" });

            var sut = new ListContentsHandler(_repo.Object, _mapper, _users.Object);
            PagedResult<ContentDto> result = await sut.Handle(new ListContentsQuery(1, 10, null), CancellationToken.None);

            result.Items.Should().HaveCount(10);
            result.Items.All(i => i.AuthorDisplayName == "Author X").Should().BeTrue();
        }
    }
}
