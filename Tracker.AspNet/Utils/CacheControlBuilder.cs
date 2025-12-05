namespace Tracker.AspNet.Utils;

public sealed class CacheControlBuilder
{
    private readonly List<string> _directives;

    public CacheControlBuilder() => _directives = [];
    public CacheControlBuilder(int capacitiy) => _directives = new(capacitiy);

    public CacheControlBuilder WithDirective(string directive)
    {
        _directives.Add(directive);
        return this;
    }

    public string Build() => $"Cache-Control: {string.Join(',', _directives)}";
}
