namespace WallMixer.Interfaces
{
	using System;
	using System.Collections.Generic;
	using System.Data.SQLite;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using DTO;
	using Dapper;

	public interface IWallpaperRepository
	{
		Task<List<WallpaperSource>> GetSourcesAsync();
		Task AddSource(WallpaperSource source);
		Task EditSource(WallpaperSource source);
		Task RemoveSource(WallpaperSource source);
		Task<WallhavenOptions> GetWallhavenOptions(WallpaperSource source);
	    Task<int> Interval();
	    Task Interval(int newInterval);
	    Task<string> SaveLocation();
	    Task SaveLocation(string newLocation);
	    Task<string> ImgurClientId();
	    Task ImgurClientId(string newId);
	}

	public class WallpaperRepository : IWallpaperRepository
	{
		private SQLiteConnection db = new SQLiteConnection("Data Source=WallMixer.sqlite;Version=3;");

		public WallpaperRepository()
		{
			ConfigureRepository();
		}

	    public async Task<int> Interval()
	    {
	        return Convert.ToInt32((await db.QueryAsync<long>("SELECT TOP 1 Interval FROM WallMixerSettings")).FirstOrDefault());
	    }

	    public async Task Interval(int newInterval)
	    {
	        await db.ExecuteAsync("UPDATE WallMixerSettings SET Interval=@interval", new {interval = newInterval});
	    }

	    public async Task<string> SaveLocation()
	    {
	        return (await db.QueryAsync<string>("SELECT TOP 1 SaveLocation FROM WallMixerSettings")).FirstOrDefault();
	    }

	    public async Task SaveLocation(string newLocation)
	    {
	        await db.ExecuteAsync("UPDATE WallMixerSettings SET SaveLocation=@location", new {location = newLocation});
	    }

	    public async Task<string> ImgurClientId()
	    {
	        return (await db.QueryAsync<string>("SELECT TOP 1 ImgurClientId FROM WallMixerSettings")).FirstOrDefault();
	    }

	    public async Task ImgurClientId(string newId)
	    {
	        await db.ExecuteAsync("UPDATE WallMixerSettings SET ImgurClientId=@clientId", new {clientId = newId});
	    }

		public async Task<List<WallpaperSource>> GetSourcesAsync()
		{
			return (await db.QueryAsync<dynamic>("SELECT Query, 0 AS Tpe FROM Subreddit UNION ALL SELECT Query, 1 AS Tpe FROM Wallhaven"))
					.Select(x => new WallpaperSource {Query = x.Query, Source = (Source) x.Tpe}).ToList();
		}

		public async Task AddSource(WallpaperSource source)
		{
			switch (source.Source)
			{
				case Source.Reddit:
					await db.ExecuteAsync("INSERT INTO Subreddit (Query) VALUES (@Query)", source);
					break;
				case Source.Wallhaven:
					await db.ExecuteAsync("INSERT INTO Wallhaven (Query, General, Anime, People, SFW, Sketchy, Resolution, Ratio) VALUES (@Query, @General, @Anime, @People, @SFW, @Sketchy, @Resolution, @Ratio)", source);
					break;
			}
		}

		public async Task EditSource(WallpaperSource source)
		{
			switch (source.Source)
			{
				case Source.Reddit:
					break;
				case Source.Wallhaven:
					await db.ExecuteAsync("UPDATE Wallhaven SET General=@General, Anime=@Anime, People=@People, SFW=@SFW, Sketchy=@Sketchy, Resolution=@Resolution, Ratio=@Ratio WHERE Query=@Query", source);
					break;
			}
		}

		public async Task RemoveSource(WallpaperSource source)
		{
			switch (source.Source)
			{
				case Source.Reddit:
					await db.ExecuteAsync("DELETE FROM Subreddit WHERE Query=@Query", source);
					break;
				case Source.Wallhaven:
					await db.ExecuteAsync("DELETE FROM Wallhaven WHERE Query=@Query", source);
					break;
			}
		}

		public async Task<WallhavenOptions> GetWallhavenOptions(WallpaperSource source)
		{
			return (await db.QueryAsync<WallhavenOptions>("SELECT TOP 1 * FROM Wallhaven WHERE Query=@Query", source)).FirstOrDefault();
		}

		private async Task ConfigureRepository()
		{
			if (File.Exists("first.bin")) return;
			await db.ExecuteAsync(@"CREATE TABLE ""WallMixerSettings"" (`SaveLocation` TEXT, `Interval` INTEGER, `ImgurClientID` TEXT)");
			await db.ExecuteAsync(@"CREATE TABLE ""Subreddit"" (`Query` TEXT)");
			await db.ExecuteAsync(@"CREATE TABLE ""Wallhaven"" (`Query` TEXT, `General` INTEGER, `Anime` INTEGER, `People` INTEGER, `SFW` INTEGER, `Sketchy` INTEGER, `Resolution` TEXT, `Ratio` TEXT)");
			await db.ExecuteAsync("INSERT INTO WallMixerSettings (SaveLocation, Interval, ImgurClientID) VALUES ('C:\\temp', 10, '1234abc')");
			new StreamWriter("first.bin");
		}
	}
}
