namespace WallMixer
{
    using System;
    using System.ComponentModel.Composition;
    using System.Drawing;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Windows;
    using Classes;
    using DTO;
    using Caliburn.Micro;
    using Interfaces;
    using Views;
    using System.Collections.Generic;

    [Export(typeof(MainViewModel))]
    public class MainViewModel : PropertyChangedBase
    {
        private IWindowManager _windowManager;
        private IWallpaperRepository _db;
        private IMetroDialog _dialog;
        private Random _rand;
        private List<string> currentImages = new List<string>();
        private List<Image> newImages = new List<Image>();
        private bool wasLocal = false;

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
            new Task(ChangeWallpaper).Start();
        }

        private async void ChangeWallpaper()
        {
            try
            {
                //TODO - 
                // Don't need internet access for local folders
                // 
                while (true)
                {
                    var sources = await _db.GetSourcesAsync();
                    var access = await NetworkTester.HasInternetAccess();
                    if (access || sources.Any(x => x.Source == Source.Local))
                    {
                        _cts = new CancellationTokenSource();

                        if (sources.Count > 0)
                        {
                            if (!wasLocal)
                                foreach (var file in currentImages)
                                    File.Delete(file);
                            currentImages.Clear();
                            OptionEnabled = true;
                            NotifyOfPropertyChange(() => OptionEnabled);
                            WallpaperSource randSource;
                            if (access)
                                randSource = sources[_rand.Next(sources.Count)];
                            else
                            {
                                sources = sources.Where(x => x.Source == Source.Local).ToList();
                                randSource = sources[_rand.Next(sources.Count)];
                            }
                            if (randSource.Source == Source.Wallhaven) randSource.WallhavenOptions = await _db.GetWallhavenOptions(randSource);
                            Image image = null;
                            var isMultiple = await _db.UseMultiple();
                            string tempFile = null;
                            wasLocal = (randSource.Source == Source.Local);
                            if (isMultiple)
                            {
                                //testing for multiple monitors
                                var screens = System.Windows.Forms.Screen.AllScreens;
                                //this needs to be modified to allow vertically stacked monitors.
                                //right now only side by side monitors will work
                                using (var bmp = new Bitmap(screens.Sum(x => x.Bounds.Width), screens.Max(x => x.Bounds.Height)))
                                using (var g = Graphics.FromImage(bmp))
                                {
                                    //get a random image for each screen
                                    foreach (var s in screens)
                                    {

                                        string imageUrl = await UrlFetcher.GetRandomImageUrl(randSource, await _db.ImgurClientId(), currentImages);
                                        if (String.IsNullOrEmpty(imageUrl))
                                        {
                                            if (String.IsNullOrEmpty(tempFile)) continue;
                                            //do nothing, leave the temp file as the current file
                                        }
                                        else
                                        {
                                            if (randSource.Source != Source.Local)
                                                tempFile = await WebImage.DownloadImage(imageUrl);
                                            else
                                                tempFile = imageUrl;
                                        }

                                        currentImages.Add(tempFile);
                                        image = Image.FromFile(tempFile);
                                        //draw it onto the screens bounds
                                        g.DrawImage(image, new Rectangle(s.Bounds.Location.X, s.Bounds.Location.Y, s.Bounds.Width, s.Bounds.Height));
                                        newImages.Add(image);
                                    }
                                    //now get the file out of it
                                    g.Save();
                                    tempFile = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "stitched.bmp");
                                    bmp.Save(tempFile);
                                }
                            }
                            else
                            {
                                string imageUrl = await UrlFetcher.GetRandomImageUrl(randSource, await _db.ImgurClientId(), currentImages);
                                if (!String.IsNullOrEmpty(imageUrl))
                                {
                                    if (randSource.Source != Source.Local)
                                        tempFile = await WebImage.DownloadImage(imageUrl);
                                    else
                                        tempFile = imageUrl;
                                    image = Image.FromFile(tempFile);
                                    newImages.Add(image);
                                    currentImages.Add(tempFile);
                                }

                            }
                            if (!currentImages.All(x => String.IsNullOrEmpty(x)))
                            {
                                DesktopBackground.SetWallpaper(tempFile, isMultiple);
                                ToolTipInfo = string.Format("Source: {0}\r\nResolution: {1}",
                                    randSource.Source == Source.Reddit ? randSource.Query : randSource.Source + " / " + randSource.Query, newImages.Aggregate("", (c, v) => $"{c}\r\n{v.Width}x{v.Height}"));
                                NotifyOfPropertyChange(() => ToolTipInfo);
                            }
                            foreach (var i in newImages)
                                i.Dispose();
                            newImages.Clear();
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
            catch (Exception e)
            {
                e = e;
            }
        }

        public void Configure()
        {
            _windowManager.ShowWindow(new ConfigureViewModel(_db, _dialog));
        }

        public async void SaveWallpaper()
        {
            foreach (var i in currentImages)
                File.Copy(i, (await _db.SaveLocation()) + @"\" + Path.GetFileName(i));
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
