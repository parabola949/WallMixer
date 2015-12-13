namespace WallMixer.Views
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Caliburn.Micro;
    using Classes;
    using Dialogs;
    using DTO;
    using Interfaces;
    using Screen = Caliburn.Micro.Screen;

    public class ConfigureViewModel : Screen
    {
        private IDatabase _db;
        private IMetroDialog _dialog;
        public IObservableCollection<WallpaperSource> Sources { get; private set; }
        public int Interval { get; set; }
        public string SaveLocation { get; set; }
        public string ImgurClientId { get; set; }
        public ICommand SetSaveLocationCommand { get { return new RelayCommand(SetSaveLocation); } }

        public ConfigureViewModel(IDatabase database, IMetroDialog metroDialog)
        {
            _db = database;
            _dialog = metroDialog;
            Sources = new BindableCollection<WallpaperSource>();
        }

        protected override async void OnInitialize()
        {
            Sources.AddRange(await _db.GetSourcesAsync());
            Interval = _db.Interval;
            SaveLocation = _db.SaveLocation;
            ImgurClientId = _db.ImgurClientId;
        }

        public async void AddSubreddit()
        {
            string sub = await _dialog.ShowInputAsync("Add new Subreddit", "Please enter subreddit name");
            if (string.IsNullOrEmpty(sub)) return;
            if (!sub.Contains("/r/")) sub = "/r/" + sub;
            var source = new WallpaperSource { Query = sub, Source = Source.Reddit };
            _db.AddSource(source);
            Sources.Add(source);
        }

        public async void AddWallhaven()
        {
            var wallhavenSource = (WallpaperSource)(await _dialog.ShowCustomDialog(new WallhavenDialog("Please enter Wallhaven query and select options")));
            if (wallhavenSource == null) return;
            if (string.IsNullOrEmpty(wallhavenSource.Query))
            {
                await _dialog.ShowMessageAsync("Error", "The search query cannot be blank.");
                return;
            }
            _db.AddSource(wallhavenSource);
            Sources.Add(wallhavenSource);

        }

        public async void EditSource(WallpaperSource source)
        {
            if (source.Source == Source.Reddit)
            {
                await _dialog.ShowMessageAsync("Error", "There are no options to edit on Subreddit sources!");
                return;
            }
            source.WallhavenOptions = await _db.GetWallhavenOptions(source);
            var wallhavenDialog = new WallhavenDialog("Edit Wallhaven Source options")
            {
                Query = source.Query,
                General = Convert.ToBoolean(source.WallhavenOptions.General),
                Anime = Convert.ToBoolean(source.WallhavenOptions.Anime),
                People = Convert.ToBoolean(source.WallhavenOptions.People),
                SFW = Convert.ToBoolean(source.WallhavenOptions.SFW),
                Sketchy = Convert.ToBoolean(source.WallhavenOptions.Sketchy),
            };
            source.WallhavenOptions.Resolution.Split(',').AsParallel().ForAll(x =>
            {
                wallhavenDialog.Resolutions.FirstOrDefault(y => y.ObjectData == x).IsSelected = true;
            });
            source.WallhavenOptions.Ratio.Split(',').AsParallel().ForAll(x =>
            {
                wallhavenDialog.Ratios.FirstOrDefault(y => y.ObjectData == x).IsSelected = true;
            });
            await _dialog.ShowCustomDialog(wallhavenDialog);
        }

        public async void DeleteSource(WallpaperSource source)
        {
            if (!await _dialog.ShowConfirmationAsync("Confirmation", string.Format("Are you sure you want to delete [{0}] from your sources?", source.Query))) return;
            _db.RemoveSource(source);
            Sources.Remove(source);
        }

        public void SetInterval()
        {
            _db.Interval = Interval;
        }

        public void SetImgurClientId()
        {
            _db.ImgurClientId = ImgurClientId;
        }

        public void SetSaveLocation()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != DialogResult.OK) return;
            _db.SaveLocation = dialog.SelectedPath;
            SaveLocation = dialog.SelectedPath;
            NotifyOfPropertyChange(() => SaveLocation);
        }
    }
}
