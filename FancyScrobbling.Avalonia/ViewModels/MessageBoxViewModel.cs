using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyScrobbling.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Avalonia.ViewModels
{
    public partial class MessageBoxViewModel : DialogViewModelBase
    {
        private readonly MainWindowViewModel _host;

        [ObservableProperty]
        private string _message;

        [ObservableProperty]
        private IEnumerable<string> _options;

        public MessageBoxViewModel(MainWindowViewModel host)
        {
            _host = host;
        }

        public override async Task LoadData(object parameter)
        {
            var parameters = (MessageBoxParameters)parameter;
            Message = parameters.Message;
            Options = parameters.Options;
        }

        private IRelayCommand _optionCommand;
        public IRelayCommand OptionCommand => _optionCommand ??= new RelayCommand<string>((parameter) =>
        {
            Complete(new DialogResult(parameter, null));
        });

        public override void Dispose()
        {

        }
    }
}
