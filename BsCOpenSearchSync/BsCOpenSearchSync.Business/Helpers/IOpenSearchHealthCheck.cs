namespace BsCOpenSearchSync.Business.Helpers;

public interface IOpenSearchHealthCheck
{
    public Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default);
}