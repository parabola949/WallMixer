namespace WallMixer.Interfaces
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using DTO;
    using HtmlAgilityPack;
    using Microsoft.Win32;
    using Newtonsoft.Json;

    public static class ImageManager
    {
        private static string _clientId;
        private const int SetDesktopWallpaper = 20;
        private const int UpdateIniFile = 0x01;
        private const int SendWinIniChange = 0x02;
        private static readonly string TempDirectory = Environment.ExpandEnvironmentVariables("%temp%");
        public static string ImageName;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public static async Task SetRandomWallpaper(WallpaperSource source, string clientId)
        {
            if (!string.IsNullOrEmpty(ImageName)) File.Delete(TempDirectory + @"\" + ImageName);
            _clientId = clientId;
            string fileName = await DownloadImage(await GetImageUrl(source));
            ImageName = fileName;
            SystemParametersInfo(SetDesktopWallpaper, 0, TempDirectory + @"\" + fileName, UpdateIniFile | SendWinIniChange);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            key.SetValue("WallpaperStyle", "2");
            key.SetValue("TileWallpaper", "0");
            key.Close();
            
        }

        private static async Task<string> DownloadImage(string url)
        {
            Uri urlUri = new Uri(url);
            string fileName = Path.GetFileName(urlUri.LocalPath);
            using (WebClient client = new WebClient())
                await client.DownloadFileTaskAsync(urlUri, TempDirectory + @"\" + fileName);
            return fileName;
        }

        private static async Task<string> GetImageUrl(WallpaperSource source)
        {
            switch (source.Source)
            {
                case Source.Reddit:
                    return await GetRandomSubredditImage(source.Query);
                case Source.Wallhaven:
                    return await GetRandomWallhavenImage(source);
            }
            return "";
        }

        private static async Task<string> GetRandomSubredditImage(string subreddit)
        {
            Random rand = new Random();
            Imgur responseData = new Imgur();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("https://api.imgur.com/3/gallery{0}/top/week", subreddit));
            request.Headers.Add("Authorization", "Client-ID " + _clientId);
            using (Stream response = (await request.GetResponseAsync()).GetResponseStream())
                using (StreamReader reader = new StreamReader(response))
                    responseData = JsonConvert.DeserializeObject<Imgur>(await reader.ReadToEndAsync());
            return responseData.data[rand.Next(responseData.data.Length)].link;
        }

        private static async Task<string> GetRandomWallhavenImage(WallpaperSource source)
        {
            Random rand = new Random();
            string baseUrl = FormatWallhavenQueryString(source.Query, source.WallhavenOptions);
            var htmlWeb = new HtmlWeb();
            var mainSearch = htmlWeb.Load(baseUrl).DocumentNode.SelectNodes("//a[@class='preview']");
            var rndImageUrl = htmlWeb.Load(mainSearch[rand.Next(mainSearch.Count - 1)].Attributes["href"].Value).DocumentNode.SelectNodes("//img[@id='wallpaper']");
            return "http:" + rndImageUrl.First().Attributes["src"].Value;
        }

        private static string FormatWallhavenQueryString(string query, WallhavenOptions options)
        {
            return
                string.Format("http://alpha.wallhaven.cc/search?q={0}&categories={1}{2}{3}&purity={4}{5}0&resolutions={6}&ratios={7}&sorting=random&order=desc",
                           query, options.General, options.Anime, options.People, options.SFW, options.Sketchy, options.Resolution.Replace(",", "%2C"), options.Ratio.Replace(",", "%2C"));
        }
    }
}
