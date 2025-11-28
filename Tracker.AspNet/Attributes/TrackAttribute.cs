namespace Tracker.AspNet.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class TrackAttribute(string route, string tables) : Attribute
{
    public string Route => route;
    public string Tables => tables;
}
