using FluentValidation;
using UserService.Application.Users.Commands;

namespace UserService.Application.Users.Command.Update
{
    public sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            When(x => x.Email is not null, () =>
            {
                RuleFor(x => x.Email!).EmailAddress().MaximumLength(200);
            });
            When(x => x.Password is not null, () =>
            {
                RuleFor(x => x.Password!).MinimumLength(6).MaximumLength(200);
            });
            When(x => x.DisplayName is not null, () =>
            {
                RuleFor(x => x.DisplayName!).NotEmpty().MaximumLength(200);
            });
        }
    }
}
