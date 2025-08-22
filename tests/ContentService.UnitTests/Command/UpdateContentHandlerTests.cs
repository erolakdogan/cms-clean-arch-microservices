using FluentAssertions;
using Moq;
using ContentService.Application.Common.Abstractions;
using ContentService.Application.Contents.Commands;
using ContentService.Domain.Entities;
namespace ContentService.Tests.Handlers
{
    public class UpdateContentHandlerTests
    {
        private readonly Mock<IContentRepository> _repo = new();
        private readonly Mock<IUnitOfWork> _uow = new();

        [Fact]
        public async Task Should_Update_When_Found()
        {
            var content = new Content { Id = Guid.NewGuid(), Title = "Old", Body = "Old", Slug = "old", Status = ContentStatus.Draft };
            _repo.Setup(r => r.GetByIdAsync(content.Id, It.IsAny<CancellationToken>())).ReturnsAsync(content);

            var sut = new UpdateContentHandler(_repo.Object, _uow.Object);
            var cmd = new UpdateContentCommand(content.Id, "New", "New body", Guid.NewGuid(), "new", ContentStatus.Published);

            await sut.Handle(cmd, CancellationToken.None);

            content.Title.Should().Be("New");
            content.Status.Should().Be(ContentStatus.Published);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
