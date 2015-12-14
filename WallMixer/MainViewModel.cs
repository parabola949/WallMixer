namespace WallMixer
{
    using System;
    using System.ComponentModel.Composition;
    using System.Drawing;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Dialogs;
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
        private string _temp = Environment.ExpandEnvironmentVariables("%temp%");
        private CancellationTokenSource _cts;

        public string ToolTipInfo { get; set; }
        
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
                _cts = new CancellationTokenSource();
                
                var sources = await _db.GetSourcesAsync();
                if (sources.Count > 0)
                {
                    var randSource = sources[_rand.Next(sources.Count)];
                    if (randSource.Source == Source.Wallhaven) randSource.WallhavenOptions = await _db.GetWallhavenOptions(randSource);
                    await ImageManager.SetRandomWallpaper(randSource, await _db.ImgurClientId());
                    ToolTipInfo = string.Format("Source: {0}", randSource.Source == Source.Reddit ? randSource.Query : randSource.Source + " / " + randSource.Query);
                    NotifyOfPropertyChange(() => ToolTipInfo);
                }
                else
                {
                    ToolTipInfo = "Please add sources!";
                    NotifyOfPropertyChange(() => ToolTipInfo);
                }
                await Task.Delay(TimeSpan.FromMinutes(await _db.Interval()), _cts.Token).ContinueWith(tsk => { });
            }
        }

        public void Configure()
        {
            _windowManager.ShowWindow(new ConfigureViewModel(_db, _dialog));
        }

        public async void SaveWallpaper()
        {
            File.Copy(_temp + @"\" + ImageManager.ImageName, (await _db.SaveLocation()) + @"\" + ImageManager.ImageName);
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
