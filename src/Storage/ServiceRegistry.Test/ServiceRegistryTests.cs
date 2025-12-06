// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.ServiceRegistry.Tests;

[TestClass]
public class ServiceRegistryTests
{
    private string _testDirectory = null!;
    private ServiceRegistryOptions _options = null!;

    [TestInitialize]
    public void Setup()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "ServiceRegistryTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);

        _options = new ServiceRegistryOptions
        {
            LibraryPath = _testDirectory,
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [TestMethod]
    public async Task InitializeAsync_CreatesDatabase()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);

        // Act
        await registry.InitializeAsync();

        // Assert
        var dbPath = _options.GetDatabasePath();
        Assert.IsTrue(File.Exists(dbPath), "Database file should be created.");
    }

    [TestMethod]
    public async Task CreateServiceAsync_CreatesNewService()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();

        // Act
        var service = await registry.CreateServiceAsync("My Books", ServiceType.Book);

        // Assert
        Assert.IsNotNull(service);
        Assert.IsFalse(string.IsNullOrEmpty(service.Id));
        Assert.AreEqual("My Books", service.Name);
        Assert.AreEqual(ServiceType.Book, service.Type);
        Assert.IsFalse(service.IsActive);
    }

    [TestMethod]
    public async Task CreateServiceAsync_CreatesDataDirectory()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();

        // Act
        var service = await registry.CreateServiceAsync("Test Service", ServiceType.Rss);

        // Assert
        var dataPath = registry.GetServiceDataPath(service.Id);
        Assert.IsTrue(Directory.Exists(dataPath), "Service data directory should be created.");
    }

    [TestMethod]
    public async Task CreateServiceAsync_WithDuplicateName_ThrowsException()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();
        await registry.CreateServiceAsync("Duplicate", ServiceType.Book);

        // Act & Assert
        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => registry.CreateServiceAsync("Duplicate", ServiceType.Book));

        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public async Task GetServiceAsync_ReturnsService()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();
        var created = await registry.CreateServiceAsync("Test", ServiceType.Podcast);

        // Act
        var retrieved = await registry.GetServiceAsync(created.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(created.Id, retrieved.Id);
        Assert.AreEqual(created.Name, retrieved.Name);
    }

    [TestMethod]
    public async Task GetServiceAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();

        // Act
        var result = await registry.GetServiceAsync("nonexistent");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAllServicesAsync_ReturnsAllServices()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();
        await registry.CreateServiceAsync("Book 1", ServiceType.Book);
        await registry.CreateServiceAsync("RSS 1", ServiceType.Rss);
        await registry.CreateServiceAsync("Podcast 1", ServiceType.Podcast);

        // Act
        var services = await registry.GetAllServicesAsync();

        // Assert
        Assert.AreEqual(3, services.Count);
    }

    [TestMethod]
    public async Task GetServicesByTypeAsync_FiltersCorrectly()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();
        await registry.CreateServiceAsync("Book 1", ServiceType.Book);
        await registry.CreateServiceAsync("Book 2", ServiceType.Book);
        await registry.CreateServiceAsync("RSS 1", ServiceType.Rss);

        // Act
        var bookServices = await registry.GetServicesByTypeAsync(ServiceType.Book);
        var rssServices = await registry.GetServicesByTypeAsync(ServiceType.Rss);

        // Assert
        Assert.AreEqual(2, bookServices.Count);
        Assert.AreEqual(1, rssServices.Count);
    }

    [TestMethod]
    public async Task SetActiveServiceAsync_SetsActiveService()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();
        var service = await registry.CreateServiceAsync("Active Test", ServiceType.Book);

        // Act
        await registry.SetActiveServiceAsync(service.Id);
        var active = await registry.GetActiveServiceAsync();

        // Assert
        Assert.IsNotNull(active);
        Assert.AreEqual(service.Id, active.Id);
        Assert.IsTrue(active.IsActive);
    }

    [TestMethod]
    public async Task SetActiveServiceAsync_OnlyOneActive()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();
        var service1 = await registry.CreateServiceAsync("Service 1", ServiceType.Book);
        var service2 = await registry.CreateServiceAsync("Service 2", ServiceType.Book);

        // Act
        await registry.SetActiveServiceAsync(service1.Id);
        await registry.SetActiveServiceAsync(service2.Id);

        var active = await registry.GetActiveServiceAsync();
        var all = await registry.GetAllServicesAsync();
        var activeCount = all.Count(s => s.IsActive);

        // Assert
        Assert.IsNotNull(active);
        Assert.AreEqual(service2.Id, active.Id);
        Assert.AreEqual(1, activeCount);
    }

    [TestMethod]
    public async Task ClearActiveServiceAsync_ClearsActiveStatus()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();
        var service = await registry.CreateServiceAsync("Test", ServiceType.Book);
        await registry.SetActiveServiceAsync(service.Id);

        // Act
        await registry.ClearActiveServiceAsync();
        var active = await registry.GetActiveServiceAsync();

        // Assert
        Assert.IsNull(active);
    }

    [TestMethod]
    public async Task UpdateServiceAsync_UpdatesService()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();
        var service = await registry.CreateServiceAsync("Original", ServiceType.Book);

        // Act
        service.Name = "Updated";
        service.Icon = "new-icon";
        service.Color = "#FF0000";
        await registry.UpdateServiceAsync(service);

        var updated = await registry.GetServiceAsync(service.Id);

        // Assert
        Assert.IsNotNull(updated);
        Assert.AreEqual("Updated", updated.Name);
        Assert.AreEqual("new-icon", updated.Icon);
        Assert.AreEqual("#FF0000", updated.Color);
    }

    [TestMethod]
    public async Task DeleteServiceAsync_DeletesService()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();
        var service = await registry.CreateServiceAsync("ToDelete", ServiceType.Book);

        // Act
        var deleted = await registry.DeleteServiceAsync(service.Id);
        var retrieved = await registry.GetServiceAsync(service.Id);

        // Assert
        Assert.IsTrue(deleted);
        Assert.IsNull(retrieved);
    }

    [TestMethod]
    public async Task DeleteServiceAsync_WithDeleteData_RemovesDirectory()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();
        var service = await registry.CreateServiceAsync("ToDelete", ServiceType.Book);
        var dataPath = registry.GetServiceDataPath(service.Id);

        // Ensure directory exists
        Assert.IsTrue(Directory.Exists(dataPath));

        // Act
        var deleted = await registry.DeleteServiceAsync(service.Id, deleteData: true);

        // Assert
        Assert.IsTrue(deleted);
        Assert.IsFalse(Directory.Exists(dataPath));
    }

    [TestMethod]
    public async Task UpdateServiceOrderAsync_UpdatesOrder()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();
        var service1 = await registry.CreateServiceAsync("First", ServiceType.Book);
        var service2 = await registry.CreateServiceAsync("Second", ServiceType.Book);
        var service3 = await registry.CreateServiceAsync("Third", ServiceType.Book);

        // Act - reverse the order
        await registry.UpdateServiceOrderAsync([service3.Id, service2.Id, service1.Id]);

        var services = await registry.GetAllServicesAsync();

        // Assert
        Assert.AreEqual(service3.Id, services[0].Id);
        Assert.AreEqual(service2.Id, services[1].Id);
        Assert.AreEqual(service1.Id, services[2].Id);
    }

    [TestMethod]
    public async Task IsServiceNameExistsAsync_ChecksCorrectly()
    {
        // Arrange
        await using var registry = new ServiceRegistry(_options);
        await registry.InitializeAsync();
        var service = await registry.CreateServiceAsync("Existing", ServiceType.Book);

        // Act & Assert
        Assert.IsTrue(await registry.IsServiceNameExistsAsync("Existing"));
        Assert.IsFalse(await registry.IsServiceNameExistsAsync("NonExisting"));
        Assert.IsFalse(await registry.IsServiceNameExistsAsync("Existing", service.Id));
    }
}
