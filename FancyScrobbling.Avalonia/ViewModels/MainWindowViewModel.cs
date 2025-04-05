using CommunityToolkit.Mvvm.ComponentModel;
using FancyScrobbling.Avalonia.Models;
using FancyScrobbling.Core;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FancyScrobbling.Avalonia.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {

        [ObservableProperty]
        private bool _isLoading = false;
        [ObservableProperty]
        private ObservableCollection<DialogViewModelBase> _dialogs = [];
        [ObservableProperty]
        private ScrobblingViewModel _scrobblingViewModel;

        public MainWindowViewModel(LastFmService lastFmService, DeviceService deviceService)
        {
            _scrobblingViewModel = new ScrobblingViewModel(this, lastFmService, deviceService);
        }

        public async Task<DialogResult> OpenDialogAsync(DialogViewModelBase dialog, object parameter)
        {
            Dialogs.Add(dialog);
            var result = await dialog.OpenAsync(parameter);
            Dialogs.Remove(dialog);
            return result;
        }
    }
}
