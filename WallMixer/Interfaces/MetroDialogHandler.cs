namespace WallMixer.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Dialogs;
    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;

    public interface IMetroDialog
    {
        Task ShowMessageAsync(string title, string message);
        Task<string> ShowInputAsync(string title, string message);
        Task<ProgressDialogController> ShowProgressAsync(string title, string message);
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task<object> ShowCustomDialog(WallhavenDialog dialog);
    }

    public sealed class MetroDialogHandler : IMetroDialog
    {
        private readonly Func<MetroWindow> ActiveWindow;

        public MetroDialogHandler(Func<MetroWindow> activeWindow)
        {
            ActiveWindow = activeWindow;
        }

        public async Task ShowMessageAsync(string title, string message)
        {
            await ActiveWindow().ShowMessageAsync(title, message);
        }

        public async Task<string> ShowInputAsync(string title, string message)
        {
            return await ActiveWindow().ShowInputAsync(title, message);
        }

        public async Task<ProgressDialogController> ShowProgressAsync(string title, string message)
        {
            return await ActiveWindow().ShowProgressAsync(title, message);
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            return await ActiveWindow().ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative;
        }

        public async Task<object> ShowCustomDialog(WallhavenDialog dialog)
        {
            await ActiveWindow().ShowMetroDialogAsync(dialog);
            var result = await dialog.WaitForButtonPressAsync();
            await ActiveWindow().HideMetroDialogAsync(dialog);
            return result;
        }
    }
}
