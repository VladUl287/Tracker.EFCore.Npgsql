using System.Reflection;
using System.Reflection.Emit;
using Tracker.Core.Services;

namespace Tracker.Core.Tests.ServicesTests;

public class AssemblyTimestampProviderTests
{
    [Fact]
    public void Default_Executing_Assembly()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyProvider = new AssemblyTimestampProvider(assembly);

        // Act
        var writeTime = assemblyProvider.GetWriteTime();
        var lastWriteTimeUtc = File.GetLastWriteTimeUtc(assembly.Location);

        // Assert
        Assert.Equal(writeTime, lastWriteTimeUtc);
    }

    [Fact]
    public void Null_Assembly()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyProvider = new AssemblyTimestampProvider(assembly);

        // Act
        var writeTime = assemblyProvider.GetWriteTime();
        var lastWriteTimeUtc = File.GetLastWriteTimeUtc(assembly.Location);

        // Assert
        Assert.Equal(writeTime, lastWriteTimeUtc);
    }

    [Fact]
    public void Dynamic_Assembly()
    {
        // Arrange
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("NullLocationAssembly"), AssemblyBuilderAccess.RunAndCollect);
        var assemblyProvider = new AssemblyTimestampProvider(assembly);

        // Act
        var getWriteTime = () => { assemblyProvider.GetWriteTime(); };

        // Assert
        Assert.Throws<FileNotFoundException>(getWriteTime);
    }

    [Fact]
    public void Dynamic_Assembly_W()
    {
        // Arrange
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("NullLocationAssembly"), AssemblyBuilderAccess.RunAndCollect);
        var assemblyProvider = new AssemblyTimestampProvider(assembly);

        // Act
        var getWriteTime = () => { assemblyProvider.GetWriteTime(); };

        // Assert
        Assert.Throws<FileNotFoundException>(getWriteTime);
    }
}
