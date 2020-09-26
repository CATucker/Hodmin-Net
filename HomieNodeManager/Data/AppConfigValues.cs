using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomieNodeManager.Data
{
    public class AppConfigValues
    {
        public MQTT mqtt { get; set; }
        public Firmware firmware { get; set; }
        public Logging logging { get; set; }
        public Output output { get; set; }
    }




    public class MQTT
    {
        public string protocol { get; set; }
        public string host { get; set; }
        public int port { get; set; }

        public bool auth { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public string base_topic { get; set; }
        public float timeout { get; set; }
    }

    public class Firmware
    {
        public string dir { get; set; }
        public string filepattern { get; set; }
    }

    public class Logging
    {
        public string logdestination { get; set; }
    }

    public class Output
    {
        public string list { get; set; }
        public string nil { get; set; }
    }
}
