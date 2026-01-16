using System.Net;
using System.Net.Http;

namespace Restaurant_Frontend_Tests.Helpers
{
    internal class DelegatingHandlerStub : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _handler;

        public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request, cancellationToken));
        }

        public static DelegatingHandlerStub FromJsonResponse(string json, HttpStatusCode statusCode = HttpStatusCode.OK, string mediaType = "application/json")
        {
            return new DelegatingHandlerStub((_, _) => new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, mediaType)
            });
        }
    }
}
