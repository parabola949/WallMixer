namespace WallMixer.Classes
{
    using System.Runtime.InteropServices;
    using Microsoft.Win32;

    public static class DesktopBackground
    {
        private const int SetDesktopWallpaper = 20;
        private const int UpdateIniFile = 0x01;
        private const int SendWinIniChange = 0x02;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public static void SetWallpaper(string filePath, bool multiple = false)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            key.SetValue("WallpaperStyle", multiple?"22":"2");//22 = span
            key.SetValue("TileWallpaper", "0");
            key.Close();
            SystemParametersInfo(SetDesktopWallpaper, 0, filePath, UpdateIniFile | SendWinIniChange);
        }
    }
}
