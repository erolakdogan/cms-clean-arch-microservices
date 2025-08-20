namespace ContentService.Domain.Entities
{
    public enum ContentStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }

    public sealed class Content
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = default!;         
        public string Body { get; set; } = default!;
        public Guid AuthorId { get; set; }   // UserService'deki User.Id'yi referans eder (cross-DB FK yok)
        public ContentStatus Status { get; set; } = ContentStatus.Draft;
        // Benzersiz, aramalarda kullanılacak. Uygulama katmanında lowercase/slugify.
        public string Slug { get; set; } = default!;         
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
