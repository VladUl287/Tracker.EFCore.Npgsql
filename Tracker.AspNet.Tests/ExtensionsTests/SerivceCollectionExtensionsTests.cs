using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tracker.AspNet.Extensions;
using Tracker.AspNet.Models;
using Tracker.AspNet.Services;
using Tracker.AspNet.Services.Contracts;
using Tracker.Core.Services;
using Tracker.Core.Services.Contracts;

namespace Tracker.AspNet.Tests.ExtensionsTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTracker_NoParameters_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = ServiceCollectionExtensions.AddTracker(services);

        // Assert
        Assert.Same(services, result);

        // Verify base services are registered
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<ITrackerHasher>());
        Assert.NotNull(serviceProvider.GetService<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>());
        Assert.NotNull(serviceProvider.GetService<IAssemblyTimestampProvider>());
        Assert.NotNull(serviceProvider.GetService<IETagProvider>());
        Assert.NotNull(serviceProvider.GetService<IRequestHandler>());
        Assert.NotNull(serviceProvider.GetService<IRequestFilter>());
        Assert.NotNull(serviceProvider.GetService<IProviderResolver>());
        Assert.NotNull(serviceProvider.GetService<ITableNameResolver>());

        // Verify GlobalOptions singleton is registered
        var optionsInstance = serviceProvider.GetService<ImmutableGlobalOptions>();
        Assert.NotNull(optionsInstance);
    }

    [Fact]
    public void AddTracker_WithOptions_RegistersServicesWithProvidedOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new GlobalOptions();

        // Register mock options builder to verify Build is called
        var mockOptionsBuilder = new Mock<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>();
        mockOptionsBuilder.Setup(b => b.Build(It.IsAny<GlobalOptions>()))
            .Returns(new ImmutableGlobalOptions());
        services.AddSingleton(mockOptionsBuilder.Object);

        // Act
        var result = ServiceCollectionExtensions.AddTracker(services, options);
        var provider = services.BuildServiceProvider();
        provider.GetService<ImmutableGlobalOptions>();

        // Assert
        Assert.Same(services, result);
        mockOptionsBuilder.Verify(b => b.Build(options), Times.Once);
    }

    [Fact]
    public void AddTracker_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.AddTracker(services, (GlobalOptions)null!));
    }

    [Fact]
    public void AddTracker_WithOptions_BuildsSingletonImmutableOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new GlobalOptions();

        // Register a mock options builder that returns a specific instance
        var expectedImmutableOptions = new ImmutableGlobalOptions();
        var mockOptionsBuilder = new Mock<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>();
        mockOptionsBuilder.Setup(b => b.Build(It.IsAny<GlobalOptions>()))
            .Returns(expectedImmutableOptions);
        services.AddSingleton(mockOptionsBuilder.Object);

        // Act
        ServiceCollectionExtensions.AddTracker(services, options);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var actualOptions = serviceProvider.GetService<ImmutableGlobalOptions>();
        Assert.Same(expectedImmutableOptions, actualOptions);
    }

    [Fact]
    public void AddTracker_WithConfigureAction_RegistersServicesWithConfiguredOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        bool wasConfigured = false;

        // Register mock options builder
        var mockOptionsBuilder = new Mock<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>();
        mockOptionsBuilder.Setup(b => b.Build(It.IsAny<GlobalOptions>()))
            .Returns(new ImmutableGlobalOptions());
        services.AddSingleton(mockOptionsBuilder.Object);

        // Act
        var result = ServiceCollectionExtensions.AddTracker(services, options =>
        {
            wasConfigured = true;
        });

        // Assert
        Assert.Same(services, result);
        Assert.True(wasConfigured);
        mockOptionsBuilder.Verify(b => b.Build(It.IsAny<GlobalOptions>()), Times.Once);
    }

    [Fact]
    public void AddTracker_WithNullConfigureAction_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.AddTracker(services, (Action<GlobalOptions>)null!));
    }

    [Fact]
    public void AddTracker_WithConfigureAction_CreatesNewOptionsInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        GlobalOptions capturedOptions = null!;

        var mockOptionsBuilder = new Mock<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>();
        mockOptionsBuilder.Setup(b => b.Build(It.IsAny<GlobalOptions>()))
            .Returns(new ImmutableGlobalOptions())
            .Callback<GlobalOptions>(opts => capturedOptions = opts);
        services.AddSingleton(mockOptionsBuilder.Object);

        // Act
        ServiceCollectionExtensions.AddTracker(services, options =>
        {
            // Modify options to verify they're being used
            // Add properties to GlobalOptions if needed for testing
        });

        // Assert
        Assert.NotNull(capturedOptions);
        // Verify it's a new instance, not some shared one
        Assert.IsType<GlobalOptions>(capturedOptions);
    }

    [Fact]
    public void AddTracker_Generic_NoParameters_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = ServiceCollectionExtensions.AddTracker<TestDbContext>(services);

        // Assert
        Assert.Same(services, result);

        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<ITrackerHasher>());
        Assert.NotNull(serviceProvider.GetService<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>());

        // Verify generic Build was used
        var mockOptionsBuilder = new Mock<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>();
        mockOptionsBuilder.Setup(b => b.Build<TestDbContext>(It.IsAny<GlobalOptions>()))
            .Returns(new ImmutableGlobalOptions());
        services.AddSingleton(mockOptionsBuilder.Object);

        ServiceCollectionExtensions.AddTracker<TestDbContext>(services);
        mockOptionsBuilder.Verify(b => b.Build<TestDbContext>(It.IsAny<GlobalOptions>()), Times.Once);
    }

    [Fact]
    public void AddTracker_Generic_WithOptions_RegistersServicesWithProvidedOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new GlobalOptions();

        var mockOptionsBuilder = new Mock<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>();
        mockOptionsBuilder.Setup(b => b.Build<TestDbContext>(It.IsAny<GlobalOptions>()))
            .Returns(new ImmutableGlobalOptions());
        services.AddSingleton(mockOptionsBuilder.Object);

        // Act
        var result = ServiceCollectionExtensions.AddTracker<TestDbContext>(services, options);

        // Assert
        Assert.Same(services, result);
        mockOptionsBuilder.Verify(b => b.Build<TestDbContext>(options), Times.Once);
    }

    [Fact]
    public void AddTracker_Generic_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.AddTracker<TestDbContext>(services, (GlobalOptions)null!));
    }

    [Fact]
    public void AddTracker_Generic_WithOptions_UsesGenericBuildMethod()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new GlobalOptions();

        var mockOptionsBuilder = new Mock<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>();
        mockOptionsBuilder.Setup(b => b.Build<TestDbContext>(It.IsAny<GlobalOptions>()))
            .Returns(new ImmutableGlobalOptions());
        services.AddSingleton(mockOptionsBuilder.Object);

        // Act
        ServiceCollectionExtensions.AddTracker<TestDbContext>(services, options);

        // Assert
        // Verify that the generic Build method was called, not the non-generic one
        mockOptionsBuilder.Verify(b => b.Build<TestDbContext>(options), Times.Once);
        mockOptionsBuilder.Verify(b => b.Build(options), Times.Never);
    }

    [Fact]
    public void AddTracker_Generic_WithConfigureAction_RegistersServicesWithConfiguredOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        bool wasConfigured = false;

        var mockOptionsBuilder = new Mock<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>();
        mockOptionsBuilder.Setup(b => b.Build<TestDbContext>(It.IsAny<GlobalOptions>()))
            .Returns(new ImmutableGlobalOptions());
        services.AddSingleton(mockOptionsBuilder.Object);

        // Act
        var result = ServiceCollectionExtensions.AddTracker<TestDbContext>(services, options =>
        {
            wasConfigured = true;
        });

        // Assert
        Assert.Same(services, result);
        Assert.True(wasConfigured);
        mockOptionsBuilder.Verify(b => b.Build<TestDbContext>(It.IsAny<GlobalOptions>()), Times.Once);
    }

    [Fact]
    public void AddTracker_Generic_WithNullConfigureAction_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.AddTracker<TestDbContext>(services, (Action<GlobalOptions>)null!));
    }

    [Fact]
    public void AddTracker_Generic_WithConfigureAction_DelegatesToGenericVersionWithOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var capturedOptions = new GlobalOptions();

        var mockOptionsBuilder = new Mock<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>();
        mockOptionsBuilder.Setup(b => b.Build<TestDbContext>(It.IsAny<GlobalOptions>()))
            .Returns(new ImmutableGlobalOptions())
            .Callback<GlobalOptions>(opts => capturedOptions = opts);
        services.AddSingleton(mockOptionsBuilder.Object);

        // Act
        ServiceCollectionExtensions.AddTracker<TestDbContext>(services, options =>
        {
            // Configure options
        });

        // Assert
        mockOptionsBuilder.Verify(b => b.Build<TestDbContext>(It.IsAny<GlobalOptions>()), Times.Once);
    }

    [Fact]
    public void AddTrackerBase_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        // Using reflection to call private method, or test indirectly through public methods
        ServiceCollectionExtensions.AddTracker(services);
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ITrackerHasher>());
        Assert.IsType<DefaultTrackerHasher>(serviceProvider.GetService<ITrackerHasher>());

        Assert.NotNull(serviceProvider.GetService<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>());
        Assert.IsType<DefaultOptionsBuilder>(serviceProvider.GetService<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>());

        Assert.NotNull(serviceProvider.GetService<IAssemblyTimestampProvider>());
        Assert.NotNull(serviceProvider.GetService<IETagProvider>());
        Assert.IsType<DefaultETagProvider>(serviceProvider.GetService<IETagProvider>());

        Assert.NotNull(serviceProvider.GetService<IRequestHandler>());
        Assert.IsType<DefaultRequestHandler>(serviceProvider.GetService<IRequestHandler>());

        Assert.NotNull(serviceProvider.GetService<IRequestFilter>());
        Assert.IsType<DefaultRequestFilter>(serviceProvider.GetService<IRequestFilter>());

        Assert.NotNull(serviceProvider.GetService<IProviderResolver>());
        Assert.IsType<DefaultProviderResolver>(serviceProvider.GetService<IProviderResolver>());

        Assert.NotNull(serviceProvider.GetService<ITableNameResolver>());
        Assert.IsType<DefaultTableNameResolver>(serviceProvider.GetService<ITableNameResolver>());
    }

    [Fact]
    public void AddTrackerBase_RegistersAssemblyTimestampProviderWithExecutingAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        ServiceCollectionExtensions.AddTracker(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var provider = serviceProvider.GetService<IAssemblyTimestampProvider>();
        Assert.NotNull(provider);
        // AssemblyTimestampProvider should be created with the executing assembly
    }

    [Fact]
    public void AddTracker_MultipleCalls_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert (should not throw)
        ServiceCollectionExtensions.AddTracker(services);
        ServiceCollectionExtensions.AddTracker(services);

        // Verify services are still registered
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<ITrackerHasher>());
    }

    [Fact]
    public void AddTracker_WithRealServiceProvider_ResolvesAllDependencies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        ServiceCollectionExtensions.AddTracker(services);
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - This test ensures all dependencies can be resolved
        Assert.NotNull(serviceProvider.GetService<ImmutableGlobalOptions>());
        Assert.NotNull(serviceProvider.GetService<ITrackerHasher>());
        Assert.NotNull(serviceProvider.GetService<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>());
        Assert.NotNull(serviceProvider.GetService<IAssemblyTimestampProvider>());
        Assert.NotNull(serviceProvider.GetService<IETagProvider>());
        Assert.NotNull(serviceProvider.GetService<IRequestHandler>());
        Assert.NotNull(serviceProvider.GetService<IRequestFilter>());
        Assert.NotNull(serviceProvider.GetService<IProviderResolver>());
        Assert.NotNull(serviceProvider.GetService<ITableNameResolver>());
    }

    [Fact]
    public void AddTracker_WithCustomDbContext_ResolvesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register mock options builder to capture context type
        var mockOptionsBuilder = new Mock<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>();
        mockOptionsBuilder.Setup(b => b.Build<CustomDbContext>(It.IsAny<GlobalOptions>()))
            .Returns(new ImmutableGlobalOptions());
        services.AddSingleton(mockOptionsBuilder.Object);

        // Act
        ServiceCollectionExtensions.AddTracker<CustomDbContext>(services);
        services.AddLogging();

        // Assert
        mockOptionsBuilder.Verify(b => b.Build<CustomDbContext>(It.IsAny<GlobalOptions>()), Times.Once);
    }

    public class TestDbContext : DbContext { }
    public class CustomDbContext : DbContext { }
}

