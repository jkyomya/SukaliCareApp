namespace SukaliCareApp.Services;

public class CgmDataService
{
	public async Task<IEnumerable<CgmReading>> GetItems()
	{
		await Task.Delay(1000); // Artifical delay to give the impression of work

		var result = new List<CgmReading>();

		for (var i = 1; i < 20; i++)
		{
            var random = new Random().Next(0,30);
            result.Add(new CgmReading
            {
                Id = i,
                ReadingDate = DateTime.Now.Subtract(TimeSpan.FromMinutes(i)),
                GlucoseValue = new GlucoseValue((float)random, "mmol/l")
            });
		}

		return result;
	}
    public async Task<IEnumerable<CgmReading>> GetItemsByDay()
    {
        await Task.Delay(1000); // Artifical delay to give the impression of work

        var result = new List<CgmReading>();

        for (var i = 1; i < 8; i++)
        {
            var random = new Random().Next(0, 30);
            result.Add(new CgmReading
            {
                Id = i,
                ReadingDate = DateTime.Now.Subtract(TimeSpan.FromDays(i)),
                GlucoseValue = new GlucoseValue((float)random, "mmol/l")
            });
        }

        return result;
    }
    public async Task<IEnumerable<CGMDevice>> GetPairedDevices()
    {
        await Task.Delay(1000); // Artifical delay to give the impression of work

        var random = new Random().Next();

        var result = new List<CGMDevice>();

        for (var i = 0; i < 4; i++)
        {
            result.Add(new CGMDevice
			{
				DeviceId = "5387842DTF",
				DeviceName = "CGM-347587539",
				DeviceKey = "3546"
			});
        }

        return result;
    }

    public async Task<IEnumerable<LibResource>> GetLibraryResources()
    {
        var result = new List<LibResource>();

        for (var i = 0; i < 6; i++)
        {
            var random = new Random().Next(1,20);
            result.Add(new LibResource
            {
                Id = random,
                Title = $"Sample reource {random}",
                Description = $"This article is an extract of resource {random}, and when tapped will take you to the website where the full article is hosted",
                Url = "https://google.com",
                Author = $"Author {random}",
                PublishedOn = DateTime.Now.Subtract(TimeSpan.FromDays(random)),

            });
        }

        return await Task.FromResult(result);
    }
}
