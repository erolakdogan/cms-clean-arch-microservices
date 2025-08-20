using FluentValidation;

namespace ContentService.Application.Contents.Commands;

public sealed class CreateContentValidator : AbstractValidator<CreateContentCommand>
{
    public CreateContentValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty();
        RuleFor(x => x.AuthorId).NotEmpty();
        When(x => x.Slug is not null, () =>
        {
            RuleFor(x => x.Slug!).NotEmpty().MaximumLength(200);
        });
    }
}
