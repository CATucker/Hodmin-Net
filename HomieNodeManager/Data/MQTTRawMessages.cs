using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomieNodeManager.Data
{
    public class MQTTRawMessage
    {

        public MQTTRawMessage(string topic, string payload)
        {
            Topic = topic;
            Payload = payload;
        }


        public string Topic { get; set; }
        public string Payload { get; set; }

    }
}
