using FluentAssertions;
using Moq;
using ContentService.Application.Common.Abstractions;
using ContentService.Application.Contents.Commands;
using ContentService.Domain.Entities;

namespace ContentService.Tests.Handlers
{
    public class CreateContentHandlerTests
    {
        private readonly Mock<IContentRepository> _repo = new();
        private readonly Mock<IUnitOfWork> _uow = new();

        [Fact]
        public async Task Should_Create_And_Return_Id()
        {
            var cmd = new CreateContentCommand("Title", "Body", Guid.NewGuid(), "title", ContentStatus.Draft);

            var sut = new CreateContentHandler(_repo.Object, _uow.Object);
            var id = await sut.Handle(cmd, CancellationToken.None);

            id.Should().NotBeEmpty();
            _repo.Verify(r => r.AddAsync(It.Is<Content>(c =>
                c.Title == cmd.Title && c.Slug == cmd.Slug), It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
