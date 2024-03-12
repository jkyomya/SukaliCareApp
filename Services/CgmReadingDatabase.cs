using SQLite;

namespace SukaliCareApp.Services;

public class CgmReadingDatabase
{
    SQLiteAsyncConnection Database;
    public CgmReadingDatabase()
    {
        
    }

    async Task Init()
    {
        if(Database is not null)
        {
            //TODO: Remove in release version
            //SEED DATA INTO THE DATABASE
            if(await Database.Table<ReadingEntity>().CountAsync() == 0)
            {
                List<ReadingEntity> seedData = [];
                for(int i = 1; i < 12; i++)
                {
                    int value = new Random().Next(1, 20);
                    seedData.Add(new ReadingEntity
                    {
                        Value = value,
                        Id = Guid.NewGuid(),
                        ReadingDate = DateTime.Now.Subtract(TimeSpan.FromMinutes(i * 30)),
                        Unit = "mmol/l",
                        UserName = "jhaby@gmail.com"
                    });

                }
                await Database.InsertAllAsync(seedData);
            }

            return;
        }

        Database = new SQLiteAsyncConnection(DbConstants.DatabasePath, DbConstants.Flags);
        var result = await Database.CreateTableAsync<ReadingEntity>();
    }

    public async Task<IEnumerable<CgmReading>> GetReadingsAsync()
    {
        await Init();
        List<ReadingEntity> readings = await Database.Table<ReadingEntity>()
            .OrderByDescending(x => x.ReadingDate)
            .ToListAsync();

        return readings.AsEnumerable()
            .Select(item => new CgmReading
            {
                Id = item.Id,
                GlucoseValue = new GlucoseValue(item.Value, item.Unit),
                ReadingDate = item.ReadingDate,
                UserId = item.UserId,
                UserName = item.UserName
            });
    }

    public async Task<IEnumerable<CgmReading>> GetDailyReadings(int days)
    {
        await Init();
        List<ReadingEntity> readings = await Database.Table<ReadingEntity>()
            .OrderByDescending(x => x.ReadingDate)
            .Take(days)
            .ToListAsync();

        return readings.AsEnumerable()
            .Select(item => new CgmReading
            {
                Id = item.Id,
                GlucoseValue = new GlucoseValue(item.Value, item.Unit),
                ReadingDate = item.ReadingDate,
                UserId = item.UserId,
                UserName = item.UserName
            });
    }
    public async Task<CgmReading> GetReading(Guid id)
    {
        await Init();
        ReadingEntity item = await Database.Table<ReadingEntity>()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        return new CgmReading
        {
            Id = item.Id,
            GlucoseValue = new GlucoseValue(item.Value, item.Unit),
            ReadingDate = item.ReadingDate,
            UserId = item.UserId,
            UserName = item.UserName
        };
    }

    public async Task<int> SaveReadingAsync(CgmReading reading)
    {
        await Init();
        return await Database.InsertAsync(new ReadingEntity
        {
            Id = Guid.NewGuid(),
            ReadingDate = reading.ReadingDate,
            Unit = reading.GlucoseValue.Unit,
            Value = reading.GlucoseValue.Value,
            UserId = reading.UserId,
            UserName = reading.UserName
        });
    }
    public async Task<int> DeleteReading(CgmReading reading)
    {
        await Init();
        return await Database.DeleteAsync(reading);
    }
}
