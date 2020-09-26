using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace HomieNodeManager.Data
{
    public class NodeListService
    {

        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Connect to MQTT Server and collect list of device properties
        /// </summary>                                                        
        // -------------------------------------------------------------------
        public static async Task<List<NodeDeviceProperties>> GetNodesAsync(AppConfigValues configValues, bool isVerbose = false)
        {
            // the raw list of messages return from the initial connection
            List<MQTTRawMessage> _DeviceListRaw = new List<MQTTRawMessage>();

            // processed device list properties
            List<NodeDeviceProperties> _DeviceProps = new List<NodeDeviceProperties>();


            // Create a new MQTT client.
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId("HodminNet")
                .WithCleanSession(); ;


            options.WithTcpServer(configValues.mqtt.host, configValues.mqtt.port);

            //TODO: Other settings here, like timeout
            options.WithCommunicationTimeout(new TimeSpan(0,0,0,0, (int)(configValues.mqtt.timeout * 1000) ));

            var _opts = options.Build();





            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                if (isVerbose)
                    Console.WriteLine($"{e.ApplicationMessage.Topic} - {e.ApplicationMessage.ConvertPayloadToString()}");

                //collect messages
                _DeviceListRaw.Add(new MQTTRawMessage(e.ApplicationMessage.Topic, e.ApplicationMessage.ConvertPayloadToString()));
            }
                );

            await mqttClient.ConnectAsync(_opts, CancellationToken.None);

            // figure base topic
            string bt = configValues.mqtt.base_topic.EndsWith("/")
                ? configValues.mqtt.base_topic
                : configValues.mqtt.base_topic + "/";

            var curtopic = new MqttTopicFilterBuilder().WithTopic(bt + "#").Build();
            await mqttClient.SubscribeAsync(curtopic);

            // sit for a short time and then disconnect
            await delaymillis(700);

            await mqttClient.DisconnectAsync();


            // now at this point, we should have a bunch of device information that
            //  was stored in MQTT
            var cnt = _DeviceListRaw.Count;


            // parse the device information out and load it into device props

            // first pass, get all the $homie messages - this will give you the
            //  base topic and the version that will be processed
            //MQTTRawMessage msg in _DeviceListRaw.OrderBy(p => p.Topic).ToList()
            foreach (MQTTRawMessage msg in _DeviceListRaw.Where(w => w.Topic.Contains("$homie")).ToList())
            {
                // create a new base message
                NodeDeviceProperties device = new NodeDeviceProperties(msg.Topic.Replace("$homie", ""));
                device.HomieVersion = msg.Payload;
                _DeviceProps.Add(device);
            }

            // now, go throught the devices found and process the remaining message values
            foreach (NodeDeviceProperties device in _DeviceProps)
            {


                // get the various values, based on the homie version
                if (device.HomieMajorVer >= 2)
                {
                    // get the name
                    device.Name = _DeviceListRaw.FirstOrDefault(r => r.Topic == device.TopicKey + "$name")?.Payload;
                    device.MAC = _DeviceListRaw.FirstOrDefault(r => r.Topic == device.TopicKey + "$mac")?.Payload;
                    device.LocalIP = _DeviceListRaw.FirstOrDefault(r => r.Topic == device.TopicKey + "$localip")?.Payload;
                    device.implmentation = _DeviceListRaw.FirstOrDefault(r => r.Topic == device.TopicKey + "$implementation")?.Payload;
                    device.config = _DeviceListRaw.FirstOrDefault(r => r.Topic == device.TopicKey + "$implementation/config")?.Payload;
                    device.FirmwareName = _DeviceListRaw.FirstOrDefault(r => r.Topic == device.TopicKey + "$fw/name")?.Payload;
                    device.FirmwareVerison = _DeviceListRaw.FirstOrDefault(r => r.Topic == device.TopicKey + "$fw/version")?.Payload;
                    device.FirmwareChecksum = _DeviceListRaw.FirstOrDefault(r => r.Topic == device.TopicKey + "$fw/checksum")?.Payload;
                }


                if (device.HomieMajorVer == 2)
                {
                    // true = device online
                    // false = device offline
                    device.Online = _DeviceListRaw.FirstOrDefault(r => r.Topic == device.TopicKey + "$online")?.Payload;
                }

                if (device.HomieMajorVer == 3)
                {
                    //
                    //init:         this is the state the device is in when it is connected to the MQTT broker, but has not yet sent all Homie messages and is not yet ready to operate.This is the first message that must that must be sent.
                    //ready:        this is the state the device is in when it is connected to the MQTT broker, has sent all Homie messages and is ready to operate.You have to send this message after all other announcements message have been sent.
                    //disconnected: this is the state the device is in when it is cleanly disconnected from the MQTT broker.You must send this message before cleanly disconnecting.
                    //sleeping:     this is the state the device is in when the device is sleeping.You have to send this message before sleeping.
                    //lost:         this is the state the device is in when the device has been “badly” disconnected.You must define this message as LWT.
                    //alert:        this is the state the device is when connected to the MQTT broker, but something wrong is happening.E.g.a sensor is not providing data and needs human intervention.You have to send this message when something is wrong.

                    var deviceState = _DeviceListRaw.FirstOrDefault(r => r.Topic == device.TopicKey + "$state")?.Payload;

                    if (deviceState == "ready")
                        device.Online = "true";
                    else
                        device.Online = "false";
                }


                if (isVerbose)
                {
                    Console.WriteLine($"  Found Device @ {device.TopicKey} : {device.Name}");
                }

            }

            return _DeviceProps;
        }


        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Wait for a short amount of time given the number of milliseconds
        /// </summary>                                                        
        // -------------------------------------------------------------------
        private static async Task delaymillis(int millis)
        {
            await Task.Delay(millis);
        }




        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Gets a list of Firmware Properties
        /// </summary>                                                        
        // -------------------------------------------------------------------
        public static List<FirmwareProperties> GetFirmwareList(AppConfigValues configValues, bool isVerbose)
        {
            List<FirmwareProperties> _retVal = null;


            // first, make sure the base path exists
            if (Directory.Exists(configValues.firmware.dir))
            {
                _retVal = new List<FirmwareProperties>();

                var dir = new DirectoryInfo(configValues.firmware.dir);

                if (isVerbose)
                {
                    Console.WriteLine($"  Searching for Firmware files ending with '{configValues.firmware.filepattern}' in '{dir.FullName}'");
                }


                // loop through the files in the firmware folder
                foreach (FileInfo file in dir.GetFiles(configValues.firmware.filepattern))
                {
                    if (isVerbose)
                        Console.Write($"    Checking {file.Name}...");


                    // get the file into a buffer to search
                    byte[] fileBytes = File.ReadAllBytes(file.FullName);

                    // scan to see if this is a Homie Firmware file by looking for "%HOMIE_ESP8266_FW%"
                    byte[] pattern = new byte[] { 0x25, 0x48, 0x4F, 0x4D, 0x49, 0x45, 0x5F, 0x45, 0x53, 0x50, 0x38, 0x32, 0x36, 0x36, 0x5F, 0x46, 0x57, 0x25 };
                    if (Extensions.IndexOfSequence(fileBytes, pattern, 0)?.Count > 0)
                    {
                        FirmwareProperties fwProps = new FirmwareProperties();


                        // found the magic bytes, get the firmware info
                        if (isVerbose)
                            Console.WriteLine($"    Found Homie Firmware, Getting Info.");

                        fwProps.Checksum = CalculateMD5Checksum(fileBytes);


                        // first, look for the name
                        fwProps.Name = System.Text.Encoding.UTF8.GetString(
                                                                    GetBytesBetweenPatterns
                                                                    (
                                                                      fileBytes
                                                                    , new byte[] { 0xBF, 0x84, 0xE4, 0x13, 0x54 }
                                                                    , new byte[] { 0x93, 0x44, 0x6B, 0xA7, 0x75 })
                                                                    );

                        // Next, look for the version 
                        fwProps.Version = System.Text.Encoding.UTF8.GetString(
                                                                    GetBytesBetweenPatterns
                                                                    (
                                                                      fileBytes
                                                                    , new byte[] { 0x6A, 0x3F, 0x3E, 0x0E, 0xE1 }
                                                                    , new byte[] { 0xB0, 0x30, 0x48, 0xD4, 0x1A })
                                                                    );

                        // Finally, see if there is a Brand set
                        fwProps.Brand = System.Text.Encoding.UTF8.GetString(
                                                                    GetBytesBetweenPatterns
                                                                    (
                                                                      fileBytes
                                                                    , new byte[] { 0xFB, 0x2A, 0xF5, 0x68, 0xC0 }
                                                                    , new byte[] { 0x6E, 0x2F, 0x0F, 0xEB, 0x2D })
                                                                    );


                        _retVal.Add(fwProps);
                    }
                    else
                    {
                        if (isVerbose)
                            Console.WriteLine($"    Not Homie Firmware, Skipping.");
                    }
                }
            }

            return _retVal;
        }



        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Find a string value between a start and end pattern
        /// </summary>                                                        
        // -------------------------------------------------------------------
        private static byte[] GetBytesBetweenPatterns(byte[] sourcebytes, byte[] startPattern, byte[] endpattern)
        {
            List<int> nameIndexs = sourcebytes.IndexOfSequence(startPattern, 0);

            if (nameIndexs != null && nameIndexs.Count > 0)
            {
                // get last, just in case, magic byte index
                int magicByteIndex = nameIndexs.Last() + startPattern.Length;

                List<int> nameEndIndexes = sourcebytes.IndexOfSequence(endpattern, magicByteIndex);

                if (nameEndIndexes != null && nameEndIndexes.Count > 0)
                {
                    // found the at least one index to start from
                    // get last index in the list, just in case there is more than one
                    int magicByteEndIndex = nameEndIndexes.Last();
                    int valueSize = magicByteEndIndex - magicByteIndex;

                    byte[] retBytes = new byte[valueSize];
                    Buffer.BlockCopy(sourcebytes, magicByteIndex, retBytes, 0, valueSize);

                    return retBytes;
                }

            }

            return null;
        }


        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Calculate an MD5 Checksum for the given file bytes
        /// </summary>                                                        
        // -------------------------------------------------------------------
        static string CalculateMD5Checksum(byte[] fileBytes)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(fileBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
