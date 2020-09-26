using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HomieNodeManager.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomieNodeManager
{
    public class Program
    {
        public static string applicationName = "Homie Node Manager";

        public static AppConfigValues ConfigValues = null;


        public static void Main(string[] args)
        {
            LoadConfiguration();

            if (ConfigValues.mqtt == null)
            {
                Console.WriteLine("No Configuration Available, quitting.");
                return;
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });




        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Retrieve the configuration from the config files
        /// </summary>                                                        
        // -------------------------------------------------------------------
        public static void LoadConfiguration()
        {
            try
            {
                String configText = System.IO.File.ReadAllText("configuration.json");
                ConfigValues = JsonSerializer.Deserialize<AppConfigValues>(configText);
            }
            catch (FileNotFoundException fnf)
            {
                // file wasn't found
                Console.WriteLine($"<f:red>ERROR: Configuration File Not Found {fnf.FileName}");
            }
        }

        //  ------------------------------------------------------------------
        /// <summary>                                                         
        /// Retrieve the configuration from the config files
        /// </summary>                                                        
        // -------------------------------------------------------------------
        public static string SaveConfiguration(string newJSONValues)
        {
            try
            {
                System.IO.File.WriteAllText("configuration.json", newJSONValues);
            }
            catch (Exception  except)
            {
                string message = $"Error Saving Configuration: {except.Message}";
                Console.WriteLine(message);
                return message;
            }

            return null;
        }

    }
}
