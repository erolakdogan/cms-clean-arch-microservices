using ContentService.Application.Common;
using FluentValidation;

namespace ContentService.Application.Contents.Command.Create
{
    public sealed class CreateContentValidator : AbstractValidator<CreateContentCommand>
    {
        public CreateContentValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Body).NotEmpty();
            RuleFor(x => x.AuthorId).NotEmpty();

            // Slug opsiyonel, varsa formatı kontrol et
            RuleFor(x => x.Slug)
                .Must(s => string.IsNullOrWhiteSpace(s) || SlugHelper.IsValidSlug(s!))
                .WithMessage("Slug must be lowercase, alphanumeric and may include single dashes.");
        }
    }
}
