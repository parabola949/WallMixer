namespace WallMixer.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Linq;
    using System.Threading.Tasks;
    using DTO;
    using Dapper;

    public interface IDatabase
    {
        Task<List<WallpaperSource>> GetSourcesAsync();
        void AddSource(WallpaperSource source);
        void EditSource(WallpaperSource source);
        void RemoveSource(WallpaperSource source);
        Task<WallhavenOptions> GetWallhavenOptions(WallpaperSource source);
        int Interval { get; set; }
        string SaveLocation { get; set; }
        string ImgurClientId { get; set; }
    }

    public class DatabaseManager : IDatabase
    {
        private SQLiteConnection db = new SQLiteConnection("Data Source=WallMixer.sqlite;Version=3;");

        public int Interval
        {
            get { return Convert.ToInt32(db.QueryAsync<long>("SELECT Interval FROM WallMixerSettings").Result.FirstOrDefault()); }
            //get { return Task.Run(async () => (await db.QueryAsync<int>("SELECT Interval FROM Settings")).FirstOrDefault()); }
            set { db.ExecuteAsync("UPDATE WallMixerSettings SET Interval=" + value); }
        }

        public string SaveLocation
        {
            get { return db.QueryAsync<string>("SELECT SaveLocation FROM WallMixerSettings").Result.FirstOrDefault(); }
            set { db.ExecuteAsync("UPDATE WallMixerSettings SET SaveLocation='" + value + "'"); }
        }

        public string ImgurClientId
        {
            get { return db.QueryAsync<string>("SELECT ImgurClientID FROM WallMixerSettings").Result.FirstOrDefault(); }
            set { db.ExecuteAsync("UPDATE WallMixerSettings SET ImgurClientID='" + value + "'"); }
        }

        public async Task<List<WallpaperSource>> GetSourcesAsync()
        {
            return (await db.QueryAsync<dynamic>("SELECT Query, 0 AS Tpe FROM Subreddit UNION ALL SELECT Query, 1 AS Tpe FROM Wallhaven"))
                    .Select(x => new WallpaperSource {Query = x.Query, Source = (Source) x.Tpe}).ToList();
        }

        public async void AddSource(WallpaperSource source)
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


        public async void EditSource(WallpaperSource source)
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

        public async void RemoveSource(WallpaperSource source)
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
            return (await db.QueryAsync<WallhavenOptions>("SELECT * FROM Wallhaven WHERE Query=@Query", source)).FirstOrDefault();
        }
    }
}
