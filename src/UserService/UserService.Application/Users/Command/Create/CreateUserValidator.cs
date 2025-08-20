using FluentValidation;

namespace UserService.Application.Users.Command.Create
{
    public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(128);
            RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Roles).NotNull().Must(r => r.Length <= 10).WithMessage("Too many roles.");
        }
    }
}
