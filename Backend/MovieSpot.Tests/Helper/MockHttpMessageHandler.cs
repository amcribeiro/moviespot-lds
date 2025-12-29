using System.Net;

namespace MovieSpot.Tests.Helper
{
    /// <summary>
    /// Mock personalizado para interceptar pedidos HTTP enviados pelo HttpClient.
    /// Permite inspecionar o conteúdo e simular respostas do servidor.
    /// </summary>
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        /// <summary>
        /// Resposta simulada que será devolvida pelo handler (por defeito, 200 OK).
        /// </summary>
        public HttpResponseMessage Response { get; set; } = new(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"success\":1}")
        };

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(Response);
        }
    }
}
