using Social_Anxiety.Core.CheckIns;

namespace Social_Anxiety.Data;

public sealed class CheckInRepository(CalmeDatabase database) : ICheckInRepository
{
    public async Task SaveForDayAsync(CheckIn checkIn)
    {
        var db = await database.GetAsync();
        var row = CheckInRow.From(checkIn);
        await db.RunInTransactionAsync(connection =>
        {
            connection.Execute("delete from checkins where LocalDay = ? and Id <> ?", row.LocalDay, row.Id);
            connection.InsertOrReplace(row);
        });
    }

    public async Task<CheckIn?> GetByDayAsync(DateOnly localDay)
    {
        var db = await database.GetAsync();
        var day = CheckInRow.Format(localDay);
        return (await db.Table<CheckInRow>().Where(r => r.LocalDay == day).FirstOrDefaultAsync())?.ToCheckIn();
    }

    public async Task<IReadOnlyList<CheckIn>> GetRangeAsync(DateOnly from, DateOnly to)
    {
        var db = await database.GetAsync();
        var rows = await db.QueryAsync<CheckInRow>(
            "select * from checkins where LocalDay between ? and ? order by LocalDay desc",
            CheckInRow.Format(from), CheckInRow.Format(to));
        return rows.Select(r => r.ToCheckIn()).ToArray();
    }

    public async Task<CheckIn?> GetByIdAsync(Guid id)
    {
        var db = await database.GetAsync();
        return (await db.FindAsync<CheckInRow>(id))?.ToCheckIn();
    }

    public async Task<IReadOnlyList<CheckIn>> GetAllAsync()
    {
        var db = await database.GetAsync();
        var rows = await db.QueryAsync<CheckInRow>("select * from checkins order by LocalDay desc");
        return rows.Select(r => r.ToCheckIn()).ToArray();
    }
}
