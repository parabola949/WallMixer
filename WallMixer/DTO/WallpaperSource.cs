namespace WallMixer.DTO
{
    public sealed class WallpaperSource
    {
        public string Query { get; set; }
        public WallhavenOptions WallhavenOptions { get; set; }
        public Source Source { get; set; }
    }

    public enum Source
    {
        Reddit, Wallhaven
    }
}
