using ContentService.Application.Common;
using FluentValidation;
namespace ContentService.Application.Contents.Command.Update
{
    public sealed class UpdateContentValidator : AbstractValidator<UpdateContentCommand>
    {
        public UpdateContentValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Body).NotEmpty();
            RuleFor(x => x.Status).NotEmpty().Must(s =>
                string.Equals(s, "Draft", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s, "Published", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s, "Archived", StringComparison.OrdinalIgnoreCase));
            RuleFor(x => x.Slug)
                .Must(s => string.IsNullOrWhiteSpace(s) || SlugHelper.IsValidSlug(s!))
                .WithMessage("Slug must be lowercase, alphanumeric and may include single dashes.");
        }
    }
}
