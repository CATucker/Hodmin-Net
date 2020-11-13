using Microsoft.CodeAnalysis.CSharp.Syntax;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
//using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;

namespace HomieNodeManager.Data
{


    // build out an MQTT Message Service
    //  it's going to listen on the given connection and 
    public delegate void MqttMessageDelegate(object sender, MqttMessageReceivedEventArgs args);

    public class MqttMessageReceivedEventArgs : EventArgs
    {
        public MQTTRawMessage NewMessage { get; }

        public MqttMessageReceivedEventArgs(MQTTRawMessage newMessage)
        {
            this.NewMessage = newMessage;
        }
    }


    //  ------------------------------------------------------------------
    /// <summary>                                                         
    /// Message Service Interface
    /// </summary>                                                        
    // -------------------------------------------------------------------
    public interface IMqttMessageReceivedService : IDisposable
    {
        Guid ServiceID { get; }
        bool IsListening { get; }

        event MqttMessageDelegate OnMqttMessage;
        //IList<MQTTRawMessage> GetAllMessages();

        void StartListening(string baseTopic);
        void StopListening();
    }







    //  ------------------------------------------------------------------
    /// <summary>                                                         
    /// Message Service Implementation
    /// </summary>                                                        
    // -------------------------------------------------------------------
    public class MqttMonitorService : IMqttMessageReceivedService
    {
        public event MqttMessageDelegate OnMqttMessage;

        // Create a new MQTT client.
        MqttFactory _factory = new MqttFactory();
        IMqttClient _mqttClient = null;
        IMqttClientOptions _opts = null;




        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Constructor
        /// </summary>                                                        
        // -------------------------------------------------------------------
        public MqttMonitorService()
        {

        }





        private Guid serviceID = Guid.NewGuid();
        private bool isListening;

        public Guid ServiceID => serviceID;


        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Determines if this instance is already listening for messages
        /// </summary>                                                        
        // -------------------------------------------------------------------
        public bool IsListening { 
            get => isListening; 
        }



        // Implment MQTT Listner
        public async void StartListening(string baseTopic)
        {
            

            var options = new MqttClientOptionsBuilder()
                .WithClientId("HodminNodeManager")
                //.WithCleanSession(); ;
                //TODO: Other settings here, like timeout
                .WithTcpServer(Program.ConfigValues.mqtt.host, Program.ConfigValues.mqtt.port);

            _opts = options.Build();
            _mqttClient = _factory.CreateMqttClient();

            _mqttClient.UseApplicationMessageReceivedHandler(e =>
                {
                    if (Program.ConfigValues.logging.isverbose)
                        Console.WriteLine($"{e.ApplicationMessage.Topic} - {e.ApplicationMessage.ConvertPayloadToString()}");

                    //Notify of received messages
                    MqttMessageReceivedEventArgs args = new MqttMessageReceivedEventArgs(new MQTTRawMessage(e.ApplicationMessage.Topic, e.ApplicationMessage.ConvertPayloadToString()));

                    if (OnMqttMessage != null)
                        OnMqttMessage(this, args);
                });

            await _mqttClient.ConnectAsync(_opts, CancellationToken.None);

            var curtopic = new MqttTopicFilterBuilder().WithTopic(baseTopic + "#").Build();

            //isListening = true;

            await _mqttClient.SubscribeAsync(curtopic);

        }




        public async void StopListening()
        {
            isListening = false;
            await _mqttClient.DisconnectAsync();
            _mqttClient.Dispose();
        }










        public void Dispose()
        {
            //TODO: Handle disposing service components
        }

    }
}
