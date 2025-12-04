namespace Tracker.AspNet.Services.Contracts;

public interface IDbOperationsFactory
{
    IDbOperations Create(string provider);
    IDbOperations Create<TContext>(string provider);
}
