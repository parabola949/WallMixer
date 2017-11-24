namespace WallMixer.Classes
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using DTO;
    using HtmlAgilityPack;
    using Newtonsoft.Json;
    using System.Security.Cryptography;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public static class UrlFetcher
    {
        private static Random _rand = new Random();
        private static string _clientId;

        public static async Task<string> GetRandomImageUrl(WallpaperSource source, string clientId, List<string> current)
        {
            _clientId = clientId;
            switch (source.Source)
            {
                case Source.Reddit:
                    return await GetRandomRedditImage(source.Query);
                case Source.Wallhaven:
                    return await GetRandomWallhavenImage(source);
                case Source.Local:
                    return await GetRandomLocalImage(source.Query, current);
            }
            return null;
        }

        private static async Task<string> GetRandomRedditImage(string subreddit)
        {
            Imgur responseData = new Imgur();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("https://api.imgur.com/3/gallery{0}/top/week", subreddit));
            request.Headers.Add("Authorization", "Client-ID " + _clientId);
            using (Stream response = (await request.GetResponseAsync()).GetResponseStream())
            using (StreamReader reader = new StreamReader(response))
                responseData = JsonConvert.DeserializeObject<Imgur>(await reader.ReadToEndAsync());
            return responseData.data[_rand.Next(responseData.data.Length)].link;
        }

        private static async Task<string> GetRandomWallhavenImage(WallpaperSource source)
        {
            string baseUrl = FormatWallhavenQueryString(source.Query, source.WallhavenOptions);
            var htmlWeb = new HtmlWeb();
            var mainSearch = htmlWeb.Load(baseUrl).DocumentNode.SelectNodes("//a[@class='preview']");
            var rndImageUrl = htmlWeb.Load(mainSearch[_rand.Next(mainSearch.Count - 1)].Attributes["href"].Value).DocumentNode.SelectNodes("//img[@id='wallpaper']");
            return "http:" + rndImageUrl.First().Attributes["src"].Value;
        }

        private static async Task<string> GetRandomLocalImage(string path, List<string> current)
        {
            //get images in the folder
            var filters = ".jpg|.jpeg|.png|.gif|.bmp$";

            var list = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Where(x => Regex.IsMatch(x, filters, RegexOptions.IgnoreCase)).Where(x => !current.Contains(x)).ToList();
            if (list.Count == 0) return null;
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            list.Shuffle();
            list.Shuffle();
            return list.First();
        }

        private static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }

        private static string FormatWallhavenQueryString(string query, WallhavenOptions options)
        {
            return
                string.Format("http://alpha.wallhaven.cc/search?q={0}&categories={1}{2}{3}&purity={4}{5}0&resolutions={6}&ratios={7}&sorting=random&order=desc",
                           query, options.General, options.Anime, options.People, options.SFW, options.Sketchy, options.Resolution.Replace(",", "%2C"), options.Ratio.Replace(",", "%2C"));
        }
    }
}
