using System.Collections.Frozen;
using Tracker.AspNet.Models;
using Tracker.AspNet.Services.Contracts;

namespace Tracker.AspNet.Services;

public class DefaultActionsRegistry : IActionsRegistry
{
    private readonly FrozenDictionary<string, ActionDescriptor> _store;

    public DefaultActionsRegistry(params ActionDescriptor[] descriptors)
    {
        var store = new Dictionary<string, ActionDescriptor>(descriptors.Length);
        foreach (var descriptor in descriptors)
            store[descriptor.Route] = descriptor;
        _store = store.ToFrozenDictionary();
    }

    public ActionDescriptor GetActionDescriptor(string route)
    {
        return _store.GetValueOrDefault(route, null);
    }
}
