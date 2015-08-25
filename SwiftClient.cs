using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using RestSharp;
using NLog;
using System.Net;

namespace SwiftLib
{
    public class SwiftClient
    {
        private SwiftConfig cfg;
        private RestClient rc;
        private String storageUrl = null;
        public String authToken = null;
        private static Logger log = LogManager.GetCurrentClassLogger();
        public SwiftClient(SwiftConfig cfg)
        {
            this.cfg = cfg;
            rc = new RestClient(cfg.Url);
            Authenticate();
        }

        public void Authenticate()
        {
            RestClient rc = GetRestClient();
            RestRequest request = new RestRequest("auth/v1.0/", Method.GET);
            request.AddHeader("X-Auth-User", cfg.User);
            request.AddHeader("X-Auth-Key", cfg.Authkey);
            IRestResponse response = rc.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent ||
                response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                foreach (Parameter hdr in response.Headers)
                {
                    if (hdr.Name.Equals("X-Storage-Url"))
                        storageUrl = hdr.Value.ToString();
                    else if (hdr.Name.Equals("X-Auth-Token"))
                        authToken = hdr.Value.ToString();
                }

                cfg.Url = storageUrl + "/default";
            }
            else
            {
                throw new Exception("Authentication Failed. Error: " + response.ToString());
            }

            Debug.Print("Storage URL:" + storageUrl + "; " + "Auth Token: " + authToken);
        }

        public Boolean IsAuthenticated()
        {
            if (storageUrl != null)
            {
                RestClient rc = GetRestClient();
                IRestRequest request = GetRequest();
                IRestResponse response = rc.Execute(request);
                return response.StatusCode == System.Net.HttpStatusCode.OK ||
                    response.StatusCode == System.Net.HttpStatusCode.NoContent;
            }

            return false;
        }

        // check if a container exists
        public Boolean ContainerExists(String containerName)
        {
            RestClient rc = GetRestClient();
            containerName = RemoveLeadingSlash(containerName);
            IRestRequest request = GetRequest(containerName, Method.HEAD);
            IRestResponse response = rc.Execute(request);
            return response.StatusCode == System.Net.HttpStatusCode.OK ||
                response.StatusCode == System.Net.HttpStatusCode.NoContent;
        }

        // create container only if not exists
        public void CreateContainer(String containerName)
        {
            CreateContainer(containerName, false);
        }

        // create a container with overwirte option
        public void CreateContainer(String containerName, Boolean overwrite)
        {
            RestClient rc = GetRestClient();
            if (ContainerExists(containerName))
            {
                if (!overwrite)
                    return;
            }

            containerName = RemoveLeadingSlash(containerName);
            IRestRequest request = GetRequest("/" + containerName, Method.PUT);
            request.AddHeader("Content-Length", "0");
            request.AddHeader("Content-Type", "application/directory");

            IRestResponse response = rc.Execute(request);



            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw new Exception("Failed to create container. Error: " + response.ToString());
        }

        public void DeleteContainer(String containerName)
        {
            RestClient rc = GetRestClient();
            if (ContainerExists(containerName))
            {
                containerName = RemoveLeadingSlash(containerName);
                IRestRequest request = GetRequest(containerName, Method.DELETE);
                request.AddHeader("Content-Length", "0");
                IRestResponse response = rc.Execute(request);
                if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
                    throw new Exception("Failed to delete container. Error: " + response.ToString());
            }
        }

        public void DeleteObject(String objectName)
        {
            RestClient rc = GetRestClient();

            objectName = RemoveLeadingSlash(objectName);
            IRestRequest request = GetRequest(objectName, Method.DELETE);
            IRestResponse response = rc.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("Failed to delete file. Error: " + response.ToString());
        }

        //public void MoveObject(string objectName, string destination)
        //{
        //    RestClient rc = GetRestClient();

        //    objectName = RemoveLeadingSlash(objectName);

        //    destination = RemoveLeadingSlash(destination);

        //    destination = destination + "/" + objectName.Split('/').LastOrDefault();

        //    IRestRequest request = GetRequest(objectName, Method.COPY);
        //    request.AddHeader("Content-Length", "0");

        //    request.AddHeader("Destination", "default/" + destination);

        //    IRestResponse response = rc.Execute(request);
        //    if (response.StatusCode != System.Net.HttpStatusCode.Created)
        //        throw new Exception("Failed to delete file. Error: " + response.ToString());

        //    //Delete source once copied to destination
        //    DeleteObject(objectName);
        //}

        // create object
        public string CreateObject(String containerName, String objectName, String inputFile)
        {
            string l_return = "";
            RestClient rc = GetRestClient();
            containerName = RemoveLeadingSlash(containerName);
            objectName = RemoveLeadingSlash(objectName);
            

            IRestRequest request = GetRequest(containerName + "/" + objectName.Replace("\\", "/"), Method.PUT);
            request.AddHeader("X-Detect-Content-Type", "true");

            request.AddFile(objectName, inputFile);

            IRestResponse response = rc.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                log.Error("Error in creating object. Error:" + response.ErrorException.StackTrace + " : " + response.ErrorException.Message);
                l_return = "Error in creating object. Error:" + response.ErrorException.StackTrace + " : " + response.ErrorException.Message;
            }
            else
            {
                log.Error(objectName.Split('.').FirstOrDefault().Replace(" ", ""));
                l_return = objectName.Split('.').FirstOrDefault().Replace(" ", "");
            }
            //file.Close();

            return l_return;
        }


        // list all objects in a container
        public List<SwiftFileInfo> GetObjectListAsObject(String containerName, bool getAll)
        {
            RestClient rc = GetRestClient();
            containerName = containerName.Trim();
            containerName = RemoveLeadingSlash(containerName);
            IRestRequest request = null;
            if (!string.IsNullOrEmpty(containerName) && !getAll)
            {
                request = GetRequest("", Method.GET, "format=json&prefix=" + containerName + "/&delimiter=/");
            }
            else if (!string.IsNullOrEmpty(containerName) && getAll)
            {
                request = GetRequest("", Method.GET, "format=json&prefix=" + containerName + "/");
            }
            else if (!getAll)
            {
                request = GetRequest("", Method.GET, "format=json&delimiter=/");
            }
            else
            {
                request = GetRequest("", Method.GET, "format=json&prefix=");
            }
            IRestResponse response = rc.Execute(request);
            return FileUtil.GetSwiftFileInfoList(response.Content);
        }

        // list all objects in a container
        public string GetObjectList(String containerName, bool getAll)
        {
            RestClient rc = GetRestClient();
            containerName = RemoveLeadingSlash(containerName);
            IRestRequest request = null;
            if (!string.IsNullOrEmpty(containerName))
            {
                request = GetRequest("", Method.GET, "format=json&prefix=" + containerName + "/&delimiter=/");
            }
            else if (!getAll)
            {
                request = GetRequest("", Method.GET, "format=json&delimiter=/");
            }
            else
            {
                request = GetRequest("", Method.GET, "format=json&prefix=");
            }
            IRestResponse response = rc.Execute(request);
            return FileUtil.GetSwiftFileInfoListAsString(response.Content);
        }

        // get file
        public void GetObject(string objectName)
        {
            RestClient rc = GetRestClient();
            IRestRequest request = GetRequest(storageUrl + "/" + objectName, Method.GET);
            IRestResponse response = rc.Execute(request);
            String fileName = Path.Combine(cfg.DownloadFolder, objectName.Replace("/", "\\"));
            FileInfo fi = new FileInfo(fileName);
            if (!Directory.Exists(fi.DirectoryName))
                Directory.CreateDirectory(fi.DirectoryName);
            File.WriteAllBytes(fileName, response.RawBytes);
        }

        // create a rest client for the given configuration
        public RestClient GetRestClient()
        {
            return new RestClient(cfg.Url);
        }

        // create a rest request for storage URL
        // and auth token in the header
        public IRestRequest GetRequest()
        {
            return GetRequest(storageUrl);
        }

        // create a rest request for resource URL
        // and auth token in the header
        public IRestRequest GetRequest(string resourceUrl)
        {
            return GetRequest(resourceUrl, Method.GET);
        }

        // create a rest request for the resource URL and the method
        // with auth token in the header
        public IRestRequest GetRequest(string resourceUrl, Method method)
        {
            return GetRequest(resourceUrl, method, null);
        }

        // create a request request for the resource URL, method and query strng
        // with auth token in header
        public IRestRequest GetRequest(string resourceUrl, Method method, string queryString)
        {
            RestRequest request = new RestRequest(resourceUrl +
                (String.IsNullOrEmpty(queryString) ? "" : "?" + queryString), method);
            request.AddHeader("X-Auth-Token", authToken);
            return request;
        }

        private String RemoveLeadingSlash(String token)
        {
            return RemoveLeadingChar(token, "/");
        }

        private String RemoveLeadingChar(String token, string chr)
        {
            while (token != null && token.Length > 0 && token.StartsWith(chr))
                token = token.Substring(1);

            return token;
        }
    }
}
