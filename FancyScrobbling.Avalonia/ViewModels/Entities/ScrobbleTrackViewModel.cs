
using CommunityToolkit.Mvvm.ComponentModel;
using FancyScrobbling.Core.Models;

namespace FancyScrobbling.Avalonia.ViewModels
{
    public partial class ScrobbleTrackViewModel : ViewModelBase
    {
        private readonly ScrobbleTrack _track;
        public ScrobbleTrack ScrobbleTrack => _track;

        [ObservableProperty]
        private int _index;
        public string Title => _track.Title;
        public string Artist => _track.Artist;
        public string Album => _track.Album;

        public ScrobbleTrackViewModel(ScrobbleTrack track)
        {
            _track = track;
        }
    }
}
