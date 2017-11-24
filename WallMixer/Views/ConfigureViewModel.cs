namespace WallMixer.Views
{
    using System;
    using System.Linq;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Caliburn.Micro;
    using Classes;
    using Dialogs;
    using DTO;
    using Interfaces;
    using Screen = Caliburn.Micro.Screen;
    using System.IO;

    public class ConfigureViewModel : Screen
    {
        private IWallpaperRepository _db;
        private IMetroDialog _dialog;
        public IObservableCollection<WallpaperSource> Sources { get; private set; }
        public int Interval { get; set; }
        public string SaveLocation { get; set; }
        public string ImgurClientId { get; set; }
        public ICommand SetSaveLocationCommand { get { return new RelayCommand(SetSaveLocation); } }
        public bool UseMultiple { get; set; }

        public ConfigureViewModel(IWallpaperRepository database, IMetroDialog metroDialog)
        {
            DisplayName = "Configure WallMixer";
            _db = database;
            _dialog = metroDialog;
            Sources = new BindableCollection<WallpaperSource>();
        }

        protected override async void OnInitialize()
        {
            Sources.AddRange(await _db.GetSourcesAsync());
            Interval = await _db.Interval();
            SaveLocation = await _db.SaveLocation();
            ImgurClientId = await _db.ImgurClientId();
            UseMultiple = await _db.UseMultiple();
        }

        public async void AddSubreddit()
        {
            string sub = await _dialog.ShowInputAsync("Add new Subreddit", "Please enter subreddit name");
            if (string.IsNullOrEmpty(sub)) return;
            if (!sub.Contains("/r/")) sub = "/r/" + sub;
            var source = new WallpaperSource { Query = sub, Source = Source.Reddit };
            await _db.AddSource(source);
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
            await _db.AddSource(wallhavenSource);
            Sources.Add(wallhavenSource);

        }

        public async void AddLocal()
        {
            var localSource = (WallpaperSource)(await _dialog.ShowCustomDialog(new LocalDialog("Please enter path and a name")));
            if (localSource == null) return;
            if (string.IsNullOrEmpty(localSource.Query))
            {
                await _dialog.ShowMessageAsync("Error", "The location cannot be blank.");
                return;
            }
            //check the directory exists
            var path = localSource.Query;
            //check if is a file
            if (File.Exists(path))
                path = Path.GetDirectoryName(path);
            if (!Directory.Exists(path))
            {
                await _dialog.ShowMessageAsync("Error", "The location cannot be found");
                return;
            }
            localSource.Query = path;
            await _db.AddSource(localSource);
            Sources.Add(localSource);
        }

        public async void EditSource(WallpaperSource source)
        {
            if (source.Source == Source.Reddit || source.Source == Source.Local)
            {
                await _dialog.ShowMessageAsync("Error", "There are no options to edit on Reddit sources!");
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
            if (!string.IsNullOrEmpty(source.WallhavenOptions.Resolution))
                source.WallhavenOptions.Resolution.Split(',').AsParallel().ForAll(x => wallhavenDialog.Resolutions.FirstOrDefault(y => y.ObjectData == x).IsSelected = true);
            if (!string.IsNullOrEmpty(source.WallhavenOptions.Ratio))
                source.WallhavenOptions.Ratio.Split(',').AsParallel().ForAll(x => wallhavenDialog.Ratios.FirstOrDefault(y => y.ObjectData == x).IsSelected = true);
            var editedSource = (WallpaperSource)await _dialog.ShowCustomDialog(wallhavenDialog);
            if (editedSource == null) return;
            if (string.IsNullOrEmpty(editedSource.Query))
            {
                await _dialog.ShowMessageAsync("Error", "The search query cannot be blank");
                return;
            }
            source = editedSource;
            await _db.EditSource(source);
        }

        public async void DeleteSource(WallpaperSource source)
        {
            if (!await _dialog.ShowConfirmationAsync("Confirmation", string.Format("Are you sure you want to delete [{0}] from your source list?", source.Query))) return;
            await _db.RemoveSource(source);
            Sources.Remove(source);
        }

        public async void SetInterval()
        {
            await _db.Interval(Interval);
        }

        public async void SetImgurClientId()
        {
            await _db.ImgurClientId(ImgurClientId);
        }

        public async void SetUseMultiple()
        {
            await _db.UseMultiple(UseMultiple);
        }

        public async void SetSaveLocation()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != DialogResult.OK) return;
            await _db.SaveLocation(dialog.SelectedPath);
            SaveLocation = dialog.SelectedPath;
            NotifyOfPropertyChange(() => SaveLocation);
        }
    }
}
