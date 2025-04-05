using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Core.Models
{
    public class Result
    {
        public bool Success { get; set; }
        public object Parameter { get; set; }
        public Result(bool success, object parameter)
        {
            Success = success; Parameter = parameter;
        }
    }
}
