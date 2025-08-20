using FluentValidation;

namespace UserService.Application.Users.Command.Update
{
    public sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Roles).NotNull().Must(r => r.Length <= 10);
        }
    }
}
