using SwiftLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HubicTesting
{
    class Program
    {

        static SwiftConfig cfg = new SwiftConfig();
        static SwiftClient client;


        static void Main(string[] args)
        {

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            cfg.Url = System.Configuration.ConfigurationManager.AppSettings.Get("SwiftUrl");
            cfg.User = System.Configuration.ConfigurationManager.AppSettings.Get("User");
            cfg.Authkey = System.Configuration.ConfigurationManager.AppSettings.Get("AuthKey");
            cfg.BoxFolder = System.Configuration.ConfigurationManager.AppSettings.Get("BoxFolder");
            cfg.DownloadFolder = System.Configuration.ConfigurationManager.AppSettings.Get("DownloadFolder");
            BuildClient();
            string container = args[0];
            if (!client.ContainerExists(container))
            {
                client.CreateContainer(container);
            }
            string[] images = Directory.GetFiles(args[1]);
            Console.WriteLine("Found {0} images", images.Count());

            Parallel.ForEach(images, s =>
            {
                Stopwatch st = Stopwatch.StartNew();
                Console.WriteLine("Uploading {0}", s);
                Console.WriteLine(client.CreateObject(container, Path.GetFileName(s), s));
                Console.WriteLine("Finished upload {0} in {1}ms", Path.GetFileName(s), st.ElapsedMilliseconds);
            });
            

        }

        public static void BuildClient()
        {
            if (client == null || !client.IsAuthenticated())
            {
                client = new SwiftClient(cfg);
            }
        }
    }
}
