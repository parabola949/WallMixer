using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallMixer.DTO;

namespace WallMixer.Dialogs
{
    public interface ICustomDialog
    {
        string Message { get; set; }
        Task<WallpaperSource> WaitForButtonPressAsync();
    }
}
