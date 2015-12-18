namespace WallMixer.Classes
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    public static class WebImage
    {
        private static readonly string Temp = Environment.ExpandEnvironmentVariables("%temp%");
        public static async Task<string> DownloadImage(string url)
        {
            Uri urlUri = new Uri(url);
            string fileName = Path.GetFileName(urlUri.LocalPath);
            using (WebClient client = new WebClient())
                await client.DownloadFileTaskAsync(urlUri, Temp + @"\" + fileName);
            return Temp + @"\" + fileName;
        }
    }
}
