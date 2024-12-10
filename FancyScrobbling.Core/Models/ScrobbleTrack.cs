
namespace FancyScrobbling.Core.Models
{
    public class ScrobbleTrack
    {
        public string Artist { get; set; }
        public string Title { get; set; }  
        public string Album { get; set; }
        public string Path { get; set; }

       
        public ScrobbleTrack() { }

        public ScrobbleTrack(string artist, string title, string album, string path)
        {
            Artist = artist;
            Title = title;
            Album = album;
            Path = path;
        }
    }
}
