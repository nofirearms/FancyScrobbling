using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Avalonia.Models
{
    public class MessageBoxParameters
    {
        public string Message { get; private set; }
        public IEnumerable<string> Options { get; private set; }
        public MessageBoxParameters(string message, IEnumerable<string> options)
        {
            Message = message;
            Options = options;
        }
    }
}
