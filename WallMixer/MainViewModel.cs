namespace WallMixer
{
    using System;
    using System.ComponentModel.Composition;
    using System.Drawing;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Classes;
    using DTO;
    using Caliburn.Micro;
    using Interfaces;
    using Views;

    [Export(typeof(MainViewModel))]
    public class MainViewModel : PropertyChangedBase
    {
        private IWindowManager _windowManager;
        private IWallpaperRepository _db;
        private IMetroDialog _dialog;
        private Random _rand;
        private string currentImage;
        private CancellationTokenSource _cts;

        public string ToolTipInfo { get; set; }
        public bool OptionEnabled { get; set; }
        
        [ImportingConstructor]
        public MainViewModel(IWindowManager manager, IWallpaperRepository db, IMetroDialog dialog)
        {
            _windowManager = manager;
            _db = db;
            _dialog = dialog;
            _rand = new Random();
            ChangeWallpaper();
        }

        private async void ChangeWallpaper()
        {
            while (true)
            {
                if (await NetworkTester.HasInternetAccess())
                {
                    _cts = new CancellationTokenSource();
                    var sources = await _db.GetSourcesAsync();
                    if (sources.Count > 0)
                    {
                        OptionEnabled = true;
                        NotifyOfPropertyChange(() => OptionEnabled);
                        var randSource = sources[_rand.Next(sources.Count)];
                        if (randSource.Source == Source.Wallhaven) randSource.WallhavenOptions = await _db.GetWallhavenOptions(randSource);
                        string imageUrl = await UrlFetcher.GetRandomImageUrl(randSource, await _db.ImgurClientId());
                        string tempFile = await WebImage.DownloadImage(imageUrl);
                        var image = Image.FromFile(tempFile);
                        DesktopBackground.SetWallpaper(tempFile);
                        ToolTipInfo = string.Format("Source: {0}\r\nResolution: {1}",
                            randSource.Source == Source.Reddit ? randSource.Query : randSource.Source + " / " + randSource.Query, image.Width + "x" + image.Height);
                        NotifyOfPropertyChange(() => ToolTipInfo);
                        image.Dispose();
                        if (!string.IsNullOrEmpty(currentImage)) File.Delete(currentImage);
                        currentImage = tempFile;
                    }
                    else
                    {
                        ToolTipInfo = "Please add sources!";
                        NotifyOfPropertyChange(() => ToolTipInfo);
                        OptionEnabled = false;
                        NotifyOfPropertyChange(() => OptionEnabled);
                    }
                    await Task.Delay(TimeSpan.FromMinutes(await _db.Interval()), _cts.Token).ContinueWith(tsk => { });
                }
                else
                {
                    ToolTipInfo = "Waiting for internet access...";
                    NotifyOfPropertyChange(() => ToolTipInfo);
                    OptionEnabled = false;
                    NotifyOfPropertyChange(() => OptionEnabled);
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
                
            }
        }

        public void Configure()
        {
            _windowManager.ShowWindow(new ConfigureViewModel(_db, _dialog));
        }

        public async void SaveWallpaper()
        {
            File.Copy(currentImage, (await _db.SaveLocation()) + @"\" + Path.GetFileName(currentImage));
        }

        public void NextWallpaper()
        {
            _cts.Cancel();
        }

        public void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
