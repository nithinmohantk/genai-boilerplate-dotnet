using BoDi;
using TechTalk.SpecFlow;
using GenAIBoilerplate.API;

namespace GenAIBoilerplate.Tests.BDD.Support;

/// <summary>
/// SpecFlow hooks for test setup and teardown
/// </summary>
[Binding]
public class Hooks
{
    private readonly IObjectContainer _objectContainer;
    private static TestWebApplicationFactory<Program>? _factory;
    private static bool _containersStarted = false;

    public Hooks(IObjectContainer objectContainer)
    {
        _objectContainer = objectContainer;
    }

    /// <summary>
    /// Setup before all tests - start containers once
    /// </summary>
    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        if (_factory == null)
        {
            _factory = new TestWebApplicationFactory<Program>();
            await _factory.StartContainersAsync();
            _containersStarted = true;
        }
    }

    /// <summary>
    /// Setup before each scenario - initialize test context
    /// </summary>
    [BeforeScenario]
    public void BeforeScenario()
    {
        // Ensure containers are started
        if (!_containersStarted || _factory == null)
        {
            throw new InvalidOperationException("Test containers are not started. Check BeforeTestRun setup.");
        }

        // Create and register test context
        var testContext = new TestContext();
        testContext.Initialize(_factory);
        _objectContainer.RegisterInstanceAs(testContext);
        
        // Register the factory for direct access if needed
        _objectContainer.RegisterInstanceAs(_factory);
    }

    /// <summary>
    /// Cleanup after each scenario - clean database and reset context
    /// </summary>
    [AfterScenario]
    public async Task AfterScenario()
    {
        if (_factory != null)
        {
            // Clean up database for next test
            await _factory.CleanupDatabaseAsync();
        }

        // Clean up test context
        var testContext = _objectContainer.Resolve<TestContext>();
        testContext?.ClearState();
        testContext?.Dispose();
    }

    /// <summary>
    /// Cleanup after all tests - dispose containers
    /// </summary>
    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (_factory != null)
        {
            await _factory.DisposeAsync();
            _factory = null;
            _containersStarted = false;
        }
    }
}
