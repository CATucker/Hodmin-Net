using HomieNodeManager.Data;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HomieNodeManager.Pages
{
    

    public partial class EditSettings : ComponentBase
    {
        const string SAVEOK = "alert-success";
        const string SAVEBAD = "alert-danger";
        const string SAVECLR = "";

        AppConfigValues configValues = null;
        string SaveStatus = null;
        string SaveStyle = null;


        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Handle the Confimation Dialog
        /// </summary>                                                        
        // -------------------------------------------------------------------
        private bool showModal = false;
        void ModalShow()
        {
            showModal = true;
        }

        void ModalCancel()
        {
            showModal = false;
        }



        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Default Constructor for the Edit Settings Page
        /// </summary>                                                        
        // -------------------------------------------------------------------
        public EditSettings()
        {
            LoadSettings();
        }


        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Handles the Form after it is submitted
        /// </summary>                                                        
        // -------------------------------------------------------------------
        void FormSubmitted()
        {
            SaveStatus = "Form submitted";

            JsonSerializerOptions opts = new JsonSerializerOptions();
            opts.WriteIndented = true;

            string configResults = JsonSerializer.Serialize(configValues, configValues.GetType(), opts);
            string statusMsg = Program.SaveConfiguration(configResults);

            // Show messaging indicating save status
            if (statusMsg == null)
            {
                SaveStatus = "Configuation Saved";
                SaveStyle = SAVEOK;
            }
            else
            {
                SaveStatus = statusMsg;
                SaveStyle = SAVEBAD;
            }

            LoadSettings();
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

            SaveStyle = SAVEOK;
            SaveStatus = "Changes Reset to saved values";
        }


        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Load Settings from Local File
        /// </summary>                                                        
        // -------------------------------------------------------------------
        private void LoadSettings()
        {
            Program.LoadConfiguration();
            configValues = Program.ConfigValues;

            SaveStyle = SAVECLR;
            SaveStatus = null;
        }

    }
}
