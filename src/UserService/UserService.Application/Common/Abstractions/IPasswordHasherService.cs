namespace UserService.Application.Common.Abstractions
{
    public interface IPasswordHasherService
    {
        string Hash(string password);
        bool Verify(string hashed, string provided);
    }
}
