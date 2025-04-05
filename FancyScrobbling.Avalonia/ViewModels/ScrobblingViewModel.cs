using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyScrobbling.Avalonia.Models;
using FancyScrobbling.Core;
using FancyScrobbling.Core.Models;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Avalonia.ViewModels
{
    public partial class ScrobblingViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _host;
        private readonly LastFmService _lastFmService;
        private readonly DeviceService _deviceService;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private IEnumerable<MediaDevice> _devices;


        private ObservableCollection<ScrobbleTrackViewModel> _tracks = [];
        public ObservableCollection<ScrobbleTrackViewModel> Tracks
        {
            get => _tracks;
            set
            {
                SetProperty(ref _tracks, value);
                Dispatcher.UIThread.Invoke(() =>
                {
                    ScrobbleCommand?.NotifyCanExecuteChanged();
                });
                _tracks.CollectionChanged += (_, _) => ScrobbleCommand?.NotifyCanExecuteChanged();
            }
        }

        [ObservableProperty]
        private List<ScrobbleTrackViewModel> _selectedTracks = [];

        private MediaDevice _device;
        public MediaDevice Device
        {
            get => _device;
            set
            {
                SetProperty(ref _device, value);
                GetTracksCommand?.NotifyCanExecuteChanged();
            }
        }

        public ScrobblingViewModel(MainWindowViewModel host, LastFmService lastFmService, DeviceService deviceService)
        {
            _host = host;
            _lastFmService = lastFmService;
            _deviceService = deviceService;



            _devices = _deviceService.GetDevices(); 
            this.AuthorizeAsync();
        }

        private async Task AuthorizeAsync()
        {
            //var logout_result = await _lastFmService.RemoveSessionFromDb();
            if (!_lastFmService.IsAuthorized)
            {
                var result = await _host.OpenDialogAsync(new AuthorizationViewModel(_host, _lastFmService), null);
            }

            var session = _lastFmService.GetSessionFromDb();
            Username = session.Name;

        }

        private IRelayCommand _scrobbleCommand;
        public IRelayCommand ScrobbleCommand => _scrobbleCommand ??= new RelayCommand(async () =>
        {
            _host.IsLoading = true;
            var result =  await Task.Run(async() =>
            {
                return await _lastFmService.ScrobbleTracksAsync(_device, _tracks.Select(o => o.ScrobbleTrack).ToList());
            });
            _host.IsLoading = false;

            if (result.Success)
            {
                await _host.OpenDialogAsync(new MessageBoxViewModel(_host), new MessageBoxParameters($"Success! Scrobbled {_tracks.Count} tracks!", ["OK"]));
                Tracks.Clear();
                SelectedTracks.Clear();
            }
            else
            {
                await _host.OpenDialogAsync(new MessageBoxViewModel(_host), new MessageBoxParameters($"{result.Parameter}", ["OK"]));
            }


            
        }, () => _tracks?.Count > 0);

        private IRelayCommand _getTracksCommand;
        public IRelayCommand GetTracksCommand => _getTracksCommand ??= new RelayCommand(async () =>
        {
            _host.IsLoading = true;

            await Task.Run(() =>
            {
                var tracks = _deviceService.GetScribbleFiles(_device);

                Tracks = new ObservableCollection<ScrobbleTrackViewModel>(tracks.Select(o => new ScrobbleTrackViewModel(o)));
                this.RefreshIndexes(Tracks);
            });

            _host.IsLoading = false;

            if (!Tracks.Any())
            {
                await _host.OpenDialogAsync(new MessageBoxViewModel(_host), new MessageBoxParameters("No tracks for scrobbling", ["OK"]));
            }

        }, () => _device != null);



        private IRelayCommand _logoutCommand;
        public IRelayCommand LogoutCommand => _logoutCommand ??= new RelayCommand(async () =>
        {
            var result = await _host.OpenDialogAsync(new MessageBoxViewModel(_host), new MessageBoxParameters("Logout?", ["Yes", "No"]));
            if(result.Result == "Yes")
            {
                Username = "";
                await _lastFmService.RemoveSessionFromDb();
                await AuthorizeAsync();
            }

        });


        private IRelayCommand _openContextMenuCommand;
        public IRelayCommand OpenContextMenuCommand => _openContextMenuCommand ??= new RelayCommand(async () =>
        {
            if (_selectedTracks is null) return;
            if (!_selectedTracks.Any()) return;

            var result = await _host.OpenDialogAsync(new MessageBoxViewModel(_host), new MessageBoxParameters("Remove selected tracks?", ["Yes", "No"]));
            if (result.Result == "Yes")
            {
                _lastFmService.RemoveScrobbleTracks(_device, _selectedTracks.Select(o => o.ScrobbleTrack));
                foreach (var track in _selectedTracks.ToList())
                {
                    _tracks.Remove(track);
                    (_selectedTracks).Remove(track);
                }
                RefreshIndexes(_tracks);
            }
        });


        private void RefreshIndexes(IList<ScrobbleTrackViewModel> tracks)
        {
            for (int i = 0; i < tracks.Count(); i++)
            {
                tracks[i].Index = i + 1;
            }
        }
    }
}
