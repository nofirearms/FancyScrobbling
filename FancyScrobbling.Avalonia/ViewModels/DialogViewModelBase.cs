using CommunityToolkit.Mvvm.ComponentModel;
using FancyScrobbling.Avalonia.Models;
using System;
using System.Threading.Tasks;

namespace FancyScrobbling.Avalonia.ViewModels
{
    public abstract partial class DialogViewModelBase : ObservableObject, IDisposable
    {
        [ObservableProperty]
        private string _header;

        [ObservableProperty]
        private bool _isOpen = false;

        [ObservableProperty]
        private bool _loaded = false;

        private TaskCompletionSource<DialogResult>? _tcs;

        public DialogViewModelBase()
        {

        }

        public async Task<DialogResult> OpenAsync(object parameter = null)
        {
            IsOpen = true;
            Loaded = false;

            _tcs = new TaskCompletionSource<DialogResult>();

            await LoadData(parameter);

            Loaded = true;

            var result = await _tcs.Task;

            IsOpen = false;

            Dispose();

            return result;
        }

        public void Complete(DialogResult result)
        {
            _tcs?.SetResult(result);
        }

        public abstract Task LoadData(object parameter);
        public abstract void Dispose();
    }
}
