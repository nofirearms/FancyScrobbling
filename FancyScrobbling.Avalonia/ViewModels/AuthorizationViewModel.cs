using CommunityToolkit.Mvvm.Input;
using FancyScrobbling.Avalonia.Models;
using FancyScrobbling.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Avalonia.ViewModels
{
    public class AuthorizationViewModel : DialogViewModelBase
    {
        private readonly MainWindowViewModel _host;
        private readonly LastFmService _lastFmService;

        public AuthorizationViewModel(MainWindowViewModel host, LastFmService lastFmService)
        {
            _host = host;
            _lastFmService = lastFmService;
        }

        public override async Task LoadData(object parameter)
        {

        }

        private IRelayCommand _authorizeCommand;
        public IRelayCommand AuthorizeCommand => _authorizeCommand ??= new RelayCommand(async () =>
        {
            var authorization_result = await _lastFmService.GetAccountAccessAsync();
            if (!authorization_result.Success)
            {
                await _host.OpenDialogAsync(new MessageBoxViewModel(_host), new MessageBoxParameters((string)authorization_result.Parameter, ["OK"]));
                return;
            }
        });

        private IRelayCommand _acceptCommand;
        public IRelayCommand AcceptCommand => _acceptCommand ??= new RelayCommand(async () =>
        {
            var result = await _lastFmService.GetSessionAsync();
            if (result.Success)
            {
                Complete(new Models.DialogResult("Success", null));
            }
            else
            {
                await _host.OpenDialogAsync(new MessageBoxViewModel(_host), new MessageBoxParameters((string)result.Parameter, ["OK"]));
            }
        });

        public override void Dispose()
        {
            
        }


    }
}
