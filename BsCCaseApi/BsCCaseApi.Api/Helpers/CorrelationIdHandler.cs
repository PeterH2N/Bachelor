namespace BsCCaseApi.Helpers;

public class CorrelationIdHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString()
                            ?? Guid.NewGuid().ToString();
        
        request.Headers.Add("X-Correlation-Id", correlationId);
        
        return base.SendAsync(request, cancellationToken);
    }
}