using FancyScrobbling.Core.Models;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Core
{
    public class DeviceService
    {

        public List<MediaDevice> GetDevices() => MediaDevice.GetDevices().ToList();

        //public List<MediaFileInfo> GetUsedFiles(MediaDevice device) => device.EnumerateUsedMediaFilesInfo("\\Internal Storage", "", SearchOption.AllDirectories).ToList();

        public List<ScrobbleTrack> GetScribbleFiles(MediaDevice device)
        {
            var items = device.EnumerateFileItems("\\Internal Storage", "", SearchOption.AllDirectories).ToList();
            //var items = EnumerateFileItems(path, searchPattern, searchOption);
            var used_items = items.Where(i => i.UseCount > 0);
            var result = new List<ScrobbleTrack>();
            foreach (var used_item in used_items)
            {
                if (string.IsNullOrEmpty(used_item.Artist) || string.IsNullOrEmpty(used_item.name)) continue;
                for (int i = 0; i < used_item.UseCount; i++)
                {
                    var scrobble_track = new ScrobbleTrack(used_item.Artist, used_item.name, used_item.Album, used_item.FullName);
                    
                    result.Add(scrobble_track);
                }
            }
            return result;
        }

    }
}
