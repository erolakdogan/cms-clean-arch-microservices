using Microsoft.AspNetCore.Identity;
using UserService.Application.Common.Abstractions;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Security
{
    public sealed class IdentityPasswordHasherService(IPasswordHasher<User> inner)
    : IPasswordHasherService
    {
        public string Hash(string password) => inner.HashPassword(new User(), password);

        public bool Verify(string hashed, string provided)
            => inner.VerifyHashedPassword(new User(), hashed, provided)
                is PasswordVerificationResult.Success
                 or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
