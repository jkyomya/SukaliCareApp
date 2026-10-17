using System.Text.Json;

namespace SukaliCareApp.Services;

/// <summary>
/// Loads the health information that ships inside the app, so it works without internet.
/// </summary>
public class HealthContentService
{
    private const string FileName = "health_content.json";
    private HealthContent cached;

    public async Task<HealthContent> GetContentAsync()
    {
        if (cached is not null)
        {
            return cached;
        }

        try
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync(FileName);
            cached = await JsonSerializer.DeserializeAsync<HealthContent>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not load health content: {ex.Message}");
        }

        return cached ?? new HealthContent();
    }
}
