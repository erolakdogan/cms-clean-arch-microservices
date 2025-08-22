using Moq;
using ContentService.Application.Common.Abstractions;
using ContentService.Application.Contents.Commands;
using ContentService.Domain.Entities;

namespace ContentService.Tests.Handlers
{
    public class DeleteContentHandlerTests
    {
        private readonly Mock<IContentRepository> _repo = new();
        private readonly Mock<IUnitOfWork> _uow = new();

        [Fact]
        public async Task Should_Delete_When_Found()
        {
            var contentId = Guid.NewGuid();
            var content = new Content { Id = contentId, Title = "Test", Body = "Body" };

            _repo.Setup(r => r.GetByIdAsync(contentId, It.IsAny<CancellationToken>())).ReturnsAsync(content);

            var sut = new DeleteContentHandler(_repo.Object, _uow.Object);
            await sut.Handle(new DeleteContentCommand(contentId), CancellationToken.None);

            _repo.Verify(r => r.Remove(content), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
