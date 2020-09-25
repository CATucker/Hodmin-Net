using HomieNodeManager.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HomieNodeManager.Pages
{
    public partial class EditSettings : ComponentBase
    {

        string ConfigJSON = null;

        AppConfigValues configValues = null;


        public EditSettings()
        {
            LoadSettings();
        }



        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Save the Changes to the Configuration File
        /// </summary>                                                        
        // -------------------------------------------------------------------
        public void SaveChanges()
        {
            string temp = configValues.mqtt.host;
        }


        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Cancel Changes to the configuation file
        /// </summary>                                                        
        // -------------------------------------------------------------------
        public void CancelChanges()
        {
            LoadSettings();
            showModal = false;
        }



        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Load Settings from Local File
        /// </summary>                                                        
        // -------------------------------------------------------------------
        private void LoadSettings()
        {
            configValues = Program.GetConfiguration();
            ConfigJSON = JsonSerializer.Serialize(configValues);
        }
    }
}
