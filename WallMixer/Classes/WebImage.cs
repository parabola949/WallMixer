namespace WallMixer.Classes
{
    using System;
    using System.Drawing;
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

        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
    }
}
