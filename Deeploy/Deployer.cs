using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DeploySettings = Deeploy.Properties.Settings;

namespace Deeploy
{
    class Deployer
    {
        private string m_Server = DeploySettings.Default.Server;
        private int m_Port = DeploySettings.Default.Port;
        private string m_Username = DeploySettings.Default.Username;
        private string m_Password = DeploySettings.Default.Password;
        private bool m_Wipe = DeploySettings.Default.WipeBeforeUpload;

        /// <summary>
        /// Upload's a directory recursively
        /// </summary>
        /// <param name="p_DirectoryPath"></param>
        /// <param name="p_UploadPath"></param>
        private void UploadRecursive(string p_DirectoryPath, string p_UploadPath)
        {
            var s_Files = Directory.GetFiles(p_DirectoryPath);

            var s_SubDirectories = Directory.GetDirectories(p_DirectoryPath);

            foreach (var l_FilePath in s_Files)
            { 
                var l_ShortPath = l_FilePath.Replace(p_DirectoryPath, string.Empty);
                
                
                var l_Data = File.ReadAllBytes(l_FilePath);
                if (!UploadFile($"{p_UploadPath}/{l_ShortPath}", l_Data))
                    continue;

                Console.WriteLine($"{l_Data.Length} bytes written.");
            }

            foreach (var l_SubDirectory in s_SubDirectories)
            {
                var l_DirectoryPath = $"{p_UploadPath}/{Path.GetFileName(l_SubDirectory)}";
                CreateDirectory(l_DirectoryPath);

                UploadRecursive(l_SubDirectory, l_DirectoryPath);
            }
        }

        /// <summary>
        /// Launch a deployment
        /// </summary>
        /// <param name="p_Manifest">Manifest</param>
        /// <param name="p_Path">Path on the host's drive</param>
        /// <returns></returns>
        public Task<bool> Deploy(Manifest p_Manifest, string p_Path)
        {
            if (string.IsNullOrWhiteSpace(p_Path))
                return Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(m_Server))
                return Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(m_Username))
                m_Username = "anonymous";

            var s_Files = Directory.GetFiles(p_Path, "*.*");
            var s_SubDirectories = Directory.GetDirectories(p_Path);

            UploadRecursive(p_Path, "/");

            return Task.FromResult(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_Path">Path on server</param>
        /// <param name="p_Data"></param>
        /// <returns></returns>
        private bool UploadFile(string p_Path, byte[] p_Data)
        {
            
            var s_Path = GetNormalizedPath($"{m_Server}/{p_Path}");
            
            var s_Uri = new Uri(s_Path, UriKind.Absolute);

            var s_Request = (FtpWebRequest)WebRequest.Create(s_Uri);
            s_Request.Method = WebRequestMethods.Ftp.UploadFile;

            s_Request.Credentials = new NetworkCredential(m_Username, m_Password);

            Console.WriteLine($"Getting stream for {s_Path}");

            try
            {
                using (var s_Stream = s_Request.GetRequestStream())
                    s_Stream.Write(p_Data, 0, p_Data.Length);


                using (var s_Response = (FtpWebResponse)s_Request.GetResponse())
                    Console.WriteLine($"Upload {s_Path} {s_Response.StatusDescription}");
            }
            catch (Exception p_Exception)
            {
                Console.WriteLine($"Exception UploadFile: {p_Exception.Message}");
                return false;
            }
            

            return true;
        }

        private bool CreateDirectory(string p_Path)
        {
            var s_Path = GetNormalizedPath($"{m_Server}/{p_Path}");

            var s_Uri = new Uri(s_Path, UriKind.Absolute);

            var s_Request = (FtpWebRequest)WebRequest.Create(s_Uri);
            s_Request.Method = WebRequestMethods.Ftp.MakeDirectory;
            s_Request.Credentials = new NetworkCredential(m_Username, m_Password);

            try
            {
                using (var s_Response = (FtpWebResponse)s_Request.GetResponse())
                {
                    Console.WriteLine(s_Response.StatusCode);
                }
            }
            catch(Exception p_Exception)
            {
                Console.WriteLine($"Error: CreateDirectory {p_Exception.Message}");
                return false;
            }

            return true;
        }

        public static string GetNormalizedPath(string p_Path)
        {
            return p_Path
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace("\\", "/").Trim();
        }
    }
}
