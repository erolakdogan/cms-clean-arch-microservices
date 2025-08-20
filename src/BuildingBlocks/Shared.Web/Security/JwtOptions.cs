namespace Shared.Web.Security
{
    public sealed class JwtOptions
    {
        public string Issuer { get; set; } = "cmspoc";
        public string Audience { get; set; } = "cmspoc.clients";
        public string Key { get; set; } = "";                
        public int AccessTokenMinutes { get; set; } = 30;
    }
}
