using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace SwiftLib
{
    public static class FileUtil
    {
        private static List<string> fileList = new List<string>();
        public static List<String> GetFiles(string folderName)
        {
            DirectoryInfo di = new DirectoryInfo(folderName);
            // iterate through all subfolders
            foreach (DirectoryInfo directory in di.GetDirectories())
            {
                GetFiles(directory.FullName);
            }

            // list all files
            foreach (FileInfo fi in di.GetFiles())
            {
                fileList.Add(fi.FullName);
            }
            return fileList;
        }

        public static List<SwiftFileInfo> GetSwiftFileInfoList(String jsonString)
        {
            List<SwiftFileInfo> fileList = new List<SwiftFileInfo>();
            String[] fileObjects = jsonString.Split("}".ToCharArray());
            foreach (String fileObj in fileObjects)
            {
                fileList.Add(ParseSwiftFileObject(fileObj));
            }

            return fileList;
        }

        public static string GetSwiftFileInfoListAsString(String jsonString)
        {
            List<SwiftFileInfo> fileList = new List<SwiftFileInfo>();

            String[] fileObjects = jsonString.Split("}".ToCharArray());

            List<string> l_return = new List<string>();

            foreach (String fileObj in fileObjects)
            {
                if (fileObj.Contains("application/directory"))
                {
                    l_return.Add(fileObj);
                    continue;
                }
                else if (!fileObj.Contains("application/directory") &&
                    (fileObj.ToUpperInvariant().Contains("BIN_") ||
                    fileObj.ToUpperInvariant().Contains("OS_") ||
                    fileObj.Split('/')[fileObj.Split('/').Length - 1].StartsWith("Acer")))
                {
                    l_return.Add(fileObj);
                    continue;
                }
                else if (!fileObj.Contains("application/directory") &&
    (fileObj.ToUpperInvariant().Contains("SD_") ||
                    fileObj.ToUpperInvariant().Contains("SDs_")))
                {
                    l_return.Add(fileObj);
                    continue;
                }

                //ParseSwiftFileObjectAsString(fileObj);
            }

            return fileObjects.ToString();
        }

        private static SwiftFileInfo ParseSwiftFileObject(String fileObj)
        {
            SwiftFileInfo sfi = new SwiftFileInfo();
            String[] tokens = fileObj.Split(",".ToCharArray());
            foreach (String token in tokens)
            {
                String tmpToken = token;
                tmpToken = tmpToken.Replace("\"", "");
                //tmpToken = tmpToken.Replace(" ", "");
                tmpToken = tmpToken.Replace("{", "");
                tmpToken = tmpToken.Replace("[", "");
                String[] keyVal = tmpToken.Split(':');

                if (keyVal[0].Trim().Replace(" ", "").Equals("hash"))
                    sfi.hash = keyVal[1].Trim();
                else if (keyVal[0].Trim().Replace(" ", "").Equals("last_modified"))
                    sfi.last_modified = keyVal[1].Trim();
                else if (keyVal[0].Trim().Replace(" ", "").Equals("bytes"))
                    sfi.bytes = keyVal[1].Trim();
                else if (keyVal[0].Trim().Replace(" ", "").Equals("name"))
                    sfi.name = keyVal[1].Trim();
                else if (keyVal[0].Trim().Replace(" ", "").Equals("content_type"))
                    sfi.content_type = keyVal[1].Trim();
            }

            return sfi;
        }

        private static List<string> ParseSwiftFileObjectAsString(String fileObj)
        {


            SwiftFileInfo sfi = new SwiftFileInfo();
            String[] tokens = fileObj.Split(",".ToCharArray());
            List<string> l_return = new List<string>();
            foreach (String token in tokens)
            {
                String tmpToken = token;
                tmpToken = tmpToken.Replace("\"", "");
                //tmpToken = tmpToken.Replace(" ", "");
                tmpToken = tmpToken.Replace("{", "");
                tmpToken = tmpToken.Replace("[", "");
                String[] keyVal = tmpToken.Split(':');

                if (keyVal[0].Trim().Replace(" ", "").Equals("hash"))
                    sfi.hash = keyVal[1];
                else if (keyVal[0].Trim().Replace(" ", "").Equals("last_modified"))
                    sfi.last_modified = keyVal[1];
                else if (keyVal[0].Trim().Replace(" ", "").Equals("bytes"))
                    sfi.bytes = keyVal[1];
                else if (keyVal[0].Trim().Replace(" ", "").Equals("name"))
                    sfi.name = keyVal[1];
                else if (keyVal[0].Trim().Replace(" ", "").Equals("content_type"))
                    sfi.content_type = keyVal[1];
            }

            return l_return;
        }
    }
}
