using FluentValidation;

namespace ContentService.Application.Contents.Commands;

public sealed class UpdateContentValidator : AbstractValidator<UpdateContentCommand>
{
    public UpdateContentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        When(x => x.Title is not null, () =>
        {
            RuleFor(x => x.Title!).NotEmpty().MaximumLength(200);
        });
        When(x => x.Slug is not null, () =>
        {
            RuleFor(x => x.Slug!).NotEmpty().MaximumLength(200);
        });
    }
}
