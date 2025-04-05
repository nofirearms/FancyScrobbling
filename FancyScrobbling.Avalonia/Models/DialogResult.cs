using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Avalonia.Models
{
    public class DialogResult
    {
        public string Result { get; set; }
        public object Parameter { get; set; }
        public DialogResult(string result, object parameter)
        {
            Result = result;
            Parameter = parameter;
        }
    }
}
