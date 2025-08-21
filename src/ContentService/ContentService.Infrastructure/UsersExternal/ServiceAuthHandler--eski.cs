//using System.Net.Http.Headers;
//using Shared.Web.Security;

//namespace ContentService.Infrastructure.UsersExternal;

//public sealed class ServiceAuthHandler(IServiceTokenProvider tokens) : DelegatingHandler
//{
//    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
//    {
//        var token = tokens.CreateServiceToken("content-service");
//        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
//        return base.SendAsync(request, ct);
//    }
//}
