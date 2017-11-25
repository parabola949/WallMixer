using System;

namespace WallMixer.DTO
{
    public sealed class WallpaperSource
    {
        public string Name { get; set; }
        public string Query { get; set; }
        public WallhavenOptions WallhavenOptions { get; set; }
        public Source Source { get; set; }
    }

    public enum Source
    {
        Reddit, Wallhaven, Local
    }

    public static class TheRandom
    {
        public static Random Random { get; set; } = new Random();
    }
}
