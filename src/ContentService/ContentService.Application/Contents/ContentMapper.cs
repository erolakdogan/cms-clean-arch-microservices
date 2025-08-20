using ContentService.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace ContentService.Application.Contents
{
    [Mapper(UseDeepCloning = false, ThrowOnMappingNullMismatch = true, RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class ContentMapper
    {
        // Entity -> DTO
        public partial ContentDto ToDto(Content entity);
        public partial List<ContentDto> ToDtoList(List<Content> items);
    }
}
