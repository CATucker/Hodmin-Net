using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomieNodeManager.Data
{
    public class NodeDeviceProperties
    {
        public NodeDeviceProperties(string topicKey)
        {
            TopicKey = topicKey;
        }

        private string _homieVersion;
        private int _homieMajor = 0;
        private int _homieMinor = 0;
        private int _homieSub = 0;


        public string TopicKey { get; set; }

        public string HomieVersion
        {
            get
            {
                return _homieVersion;
            }
            set
            {
                _homieVersion = value;

                var pieces = value.Split('.', 3);

                //TODO: Assume there could be some problems here

                int.TryParse(pieces[0], out _homieMajor);
                int.TryParse(pieces[1], out _homieMinor);
                int.TryParse(pieces[2], out _homieSub);
            }
        }
        public int HomieMajorVer { get { return _homieMajor; } }
        public int HomieMinorVer { get { return _homieMinor; } }
        public int HomieSubVer { get { return _homieSub; } }

        public string Name { get; set; }

        private string _mac;
        public string MAC
        {
            get
            {
                return _mac;
            }
            set
            {
                //_mac = value.Replace(":", "").ToLower();
                _mac = value.ToLower();
            }
        }
        public string LocalIP { get; set; }
        public string Online { get; set; }


        public string FirmwareName { get; set; }
        public string FirmwareVerison { get; set; }
        public string FirmwareChecksum { get; set; }


        public string implmentation { get; set; }
        public string config { get; set; }








        // ViewModel Properties
        public bool isConfigShown { get; set; } = false;

    }
}
