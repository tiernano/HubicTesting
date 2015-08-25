SwiftLib_Hubic
==============
Modded SwiftLib to work with [Hubic.com](https://hubic.com)


----------


***Log4Net is needed***


----------


You first need a swift proxy for hubic. Ex.: [Hubic2SwiftGate](https://github.com/oderwat/hubic2swiftgate)
This proxy will be the entry point to get a Hubic connection token and Hubic URL to perform calls.

Needed key in .config :

    <add key="SwiftUrl" value="proxyURL" />
    <add key="User" value="proxyUser" />
    <add key="AuthKey" value="proxyPWD" />
    <add key="BoxFolder" value="c:\box" />
    <add key="DownloadFolder" value="c:\boxd" />
    

Code :

        static SwiftConfig cfg = new SwiftConfig();
        static SwiftClient client;
        
        private static void InitConfig()
        {
            cfg.Url = System.Configuration.ConfigurationManager.AppSettings.Get("SwiftUrl");
            cfg.User = System.Configuration.ConfigurationManager.AppSettings.Get("User");
            cfg.Authkey = System.Configuration.ConfigurationManager.AppSettings.Get("AuthKey");
            cfg.BoxFolder = System.Configuration.ConfigurationManager.AppSettings.Get("BoxFolder");
            cfg.DownloadFolder = System.Configuration.ConfigurationManager.AppSettings.Get("DownloadFolder");
        }

        // build Swift client using the authentication
        // information in the configuration file
        public static void BuildClient()
        {
            if (client == null || !client.IsAuthenticated())
            {
                client = new SwiftClient(cfg);
            }
        }        
        


----------


Create object

    client.CreateObject(hubic_container, filename, "", local_file_path);


----------


Create container

    client.CreateContainer(path);


----------


Delete container/object

    client.DeleteContainer(path);


----------

Get objects/containers list

    client.GetObjectListAsObject(path, true/false);


----------


If you have issues uploading large files, try increasing executionTimeout & maxRequestLength.
Ex.: 

    <httpRuntime targetFramework="4.5" executionTimeout="14400" maxRequestLength="2147483647"/>
