namespace WallMixer
{
    using Caliburn.Micro;
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.Linq;
    using System.Windows;
    using Interfaces;
    using MahApps.Metro.Controls;

    public class MainBootstrapper : BootstrapperBase
    {
        private CompositionContainer container;

        public MainBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            container = new CompositionContainer(new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));
            CompositionBatch batch = new CompositionBatch();
            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IWallpaperRepository>(new WallpaperRepository());
            batch.AddExportedValue<IMetroDialog>(new MetroDialogHandler(() => Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow as MetroWindow));
            //batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(container);
            container.Compose(batch);
        }

        protected override object GetInstance(Type service, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(service) : key;
            var exports = container.GetExportedValues<object>(contract);
            if (exports.Any()) return exports.First();
            throw new Exception(string.Format("Could not locate any instances of contract {0}", contract));
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }
    }
}