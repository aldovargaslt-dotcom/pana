using System.Text.Json;
using Microsoft.Playwright;

namespace Pana.Tests.E2E;

/// <summary>
/// End-to-end UI tests using Playwright.
/// Run against localhost: dotnet test --filter "FullyQualifiedName~E2E"
/// Requires: dotnet run running on localhost:5202
/// </summary>
public class InventoryE2ETests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private const string BaseUrl = "http://127.0.0.1:5202";
    private string _testEmail = $"e2e-{Guid.NewGuid():N}@pana.test";
    private const string TestPassword = "E2eTest123!";

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
        _context = await _browser.NewContextAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    /// <summary>
    /// Register a test user via API, then log in via the web form to get the auth cookie.
    /// </summary>
    private async Task<IPage> LoginAndGetPageAsync()
    {
        // Step 1: Register via API
        var apiPage = await _context.NewPageAsync();
        var payload = JsonSerializer.Serialize(new
        {
            email = _testEmail,
            password = TestPassword,
            displayName = "E2E Test User"
        });

        var response = await apiPage.APIRequest.PostAsync($"{BaseUrl}/api/auth/register", new APIRequestContextOptions
        {
            Data = payload,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        });

        // Register may fail if user exists — that's fine, proceed to login
        await apiPage.CloseAsync();

        // Step 2: Log in via web form to get cookie set
        var page = await _context.NewPageAsync();
        await page.GotoAsync($"{BaseUrl}/auth/login");

        await page.FillAsync("input[name='email']", _testEmail);
        await page.FillAsync("input[name='password']", TestPassword);
        await page.ClickAsync("button[type='submit']");

        // Wait for redirect to dashboard (/) after login
        await page.WaitForURLAsync($"{BaseUrl}/", new PageWaitForURLOptions
        {
            Timeout = 5000,
        });

        return page;
    }

    [Fact]
    public async Task InventoryPage_Loads_AndShowsDailyActions()
    {
        // Arrange — login first
        var page = await LoginAndGetPageAsync();

        // Act — navigate to inventory
        await page.GotoAsync($"{BaseUrl}/inventory");

        // Assert — page title contains Pana
        var title = await page.TitleAsync();
        Assert.Contains("Pana", title);

        // Assert — the three action buttons exist (tablet-first layout)
        await Assertions.Expect(page.Locator("button:has-text('AGREGAR PRODUCCIÓN')").First).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("button:has-text('Inicial')").First).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("button:has-text('Devolución')").First).ToBeVisibleAsync();

        // Assert — daily metrics exist (compact KPI bar)
        await Assertions.Expect(page.Locator("text=Sobrante").First).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("text=Producido").First).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("text=Devolución").First).ToBeVisibleAsync();

        // Screenshot for visual verification
        var screenshotDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "E2E_Screenshots");
        Directory.CreateDirectory(screenshotDir);
        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(screenshotDir, "inventory_daily.png"),
            FullPage = true,
        });

        await page.CloseAsync();
    }

    [Fact]
    public async Task InventoryPage_HasDayStatusOpen()
    {
        var page = await LoginAndGetPageAsync();
        await page.GotoAsync($"{BaseUrl}/inventory");

        // The day should be open (not cerrado)
        await Assertions.Expect(page.Locator("text=Abierto").First).ToBeVisibleAsync();

        // Cerrar día button should exist
        await Assertions.Expect(page.Locator("button:has-text('Cerrar día')").First).ToBeVisibleAsync();

        await page.CloseAsync();
    }
}
