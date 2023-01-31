using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPlayAndPTZDemo
{
    public class MapDeviceCred
    {
        public string IP { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DeviceType { get; set; }
        public string DeviceId { get; set; }
        public List<NDVRChannel> ndvr { get; set; }
    }
}
