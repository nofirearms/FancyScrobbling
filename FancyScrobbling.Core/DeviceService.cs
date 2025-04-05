using FancyScrobbling.Core.Models;
using MediaDevices;
using MediaDevices.Internal;
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

        public bool ConnectDevice(MediaDevice device)
        {
            if (device is null) return false;
            device.Connect();
            return true;
        }

        public void DisconnectDevice(MediaDevice device)
        {
            device.Disconnect(); 
        }



        public List<ScrobbleTrack> GetScribbleFiles(MediaDevice device)
        {
            if (device is null) return new List<ScrobbleTrack>();

            if (!device.IsConnected)
            {
                device.Connect();
            }

            var items = new List<Item>();
            //Перебераем диски, на плеере может быть Internal storage и External storage (флеха)
            foreach(var drive in device.GetDrives())
            {
                var drive_items = device.EnumerateFileItems(drive.Name, "", SearchOption.AllDirectories).ToList();
                items.AddRange(drive_items);
            }

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
