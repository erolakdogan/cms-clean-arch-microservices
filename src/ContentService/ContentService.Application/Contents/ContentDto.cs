namespace ContentService.Application.Contents
{
    public sealed record ContentDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = default!;
        public string Body { get; init; } = default!;
        public Guid AuthorId { get; init; }
        public string Status { get; init; } = default!;
        public string Slug { get; init; } = default!;
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }

        // Enrichment (UserService’den)
        public string? AuthorDisplayName { get; init; }
        public string? AuthorEmail { get; init; }

    }
}
