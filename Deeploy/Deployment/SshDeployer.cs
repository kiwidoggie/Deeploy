using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Deeploy.Deployment
{
    /// <summary>
    /// Deployer for SSH based solution
    /// </summary>
    class SshDeployer : Deployer
    {
        /// <summary>
        /// Ssh connection information
        /// </summary>
        protected ConnectionInfo Information { get; set; }

        protected bool Failed { get; set; }

        internal SshDeployer() : base()
        {
            var s_AuthenticationList = new List<AuthenticationMethod>();

            if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password))
                s_AuthenticationList.Add(new PasswordAuthenticationMethod(Username, Password));

            if (!string.IsNullOrWhiteSpace(CertPath))
                s_AuthenticationList.Add(new PrivateKeyAuthenticationMethod(Username, new PrivateKeyFile[]
                {
                    new PrivateKeyFile(CertPath, Password)
                }));

            // Check to ensure that we have any authentication method set, otherwise we will get an exception
            if (!s_AuthenticationList.Any())
            {
                Console.WriteLine($"There is no authentication method set.");
                Environment.Exit(-1);
            }

            Information = new ConnectionInfo(Server, Username, s_AuthenticationList.ToArray());
        }

        /// <summary>
        /// Creates a directory
        /// </summary>
        /// <param name="p_Path">Path on the remote server</param>
        /// <returns>True on success, false otherwise</returns>
        public override bool CreateDirectory(string p_Path)
        {
            var s_Path = GetNormalizedPath(p_Path);

            try
            {
                using (var s_Client = new SshClient(Information))
                {
                    if (!s_Client.IsConnected)
                        s_Client.Connect();

                    using (var s_Command = s_Client.CreateCommand($"mkdir -p /{s_Path} && chmod +rw /{s_Path}"))
                    {
                        s_Command.Execute();
#if DEBUG
                        Console.WriteLine("Command>" + s_Command.CommandText);
                        Console.WriteLine("Return Value = {0}", s_Command.ExitStatus);
#endif
                    }

                    s_Client.Disconnect();

                    return true;
                }
            }
            catch (Exception p_Exception)
            {
                Failed = true;
                Console.WriteLine($"CreateDirectory Exception: {p_Exception.Message}");
                return false;
            }
        }

        /// <summary>
        /// Launch a deployment
        /// </summary>
        /// <param name="p_Manifest">Manifest</param>
        /// <param name="p_Path">Path on the host's drive</param>
        /// <returns></returns>
        public override Task<bool> Deploy(Manifest p_Manifest, string p_LocalPath)
        {
            Failed = false;

            if (string.IsNullOrWhiteSpace(p_LocalPath))
                return Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(Server))
                return Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(Username))
                Username = "anonymous";

            var s_Files = Directory.GetFiles(p_LocalPath, "*.*");
            var s_SubDirectories = Directory.GetDirectories(p_LocalPath);

            UploadRecursive(p_LocalPath, "/home/deeploy/_deployment");

            return Task.FromResult(!Failed);
        }

        /// <summary>
        /// Uploads a file
        /// </summary>
        /// <param name="p_Path">Path on server</param>
        /// <param name="p_Data"></param>
        /// <returns>True on success, false otherwise</returns>
        public override bool UploadFile(string p_Path, byte[] p_Data)
        {
            try
            {
                using (var s_Client = new SftpClient(Information))
                {
                    s_Client.Connect();

                    var s_Path = "/" + GetNormalizedPath(p_Path);
                    
                    using (var s_Stream = new MemoryStream(p_Data))
                        s_Client.UploadFile(s_Stream, s_Path, true);

                    s_Client.Disconnect();

                    return true;
                }
            }
            catch (Exception p_Exception)
            {
                Failed = true;
                Console.WriteLine($"UploadFile Exception: {p_Path} {p_Exception.Message}");
                return false;
            }
        }

        /// <summary>
        /// Upload's a directory recursively
        /// </summary>
        /// <param name="p_DirectoryPath"></param>
        /// <param name="p_UploadPath"></param>
        public override void UploadRecursive(string p_LocalDirectoryPath, string p_RemoteDirectoryPath)
        {
            var s_Files = Directory.GetFiles(p_LocalDirectoryPath);

            var s_SubDirectories = Directory.GetDirectories(p_LocalDirectoryPath);

            foreach (var l_FilePath in s_Files)
            {
                var l_ShortPath = GetNormalizedPath(l_FilePath.Replace(p_LocalDirectoryPath, string.Empty));


                var l_Data = File.ReadAllBytes(l_FilePath);
                var l_Path = $"{p_RemoteDirectoryPath}/{l_ShortPath}";
                if (!UploadFile(l_Path, l_Data))
                    continue;

                Console.WriteLine($"{l_Path} {l_Data.Length} bytes written.");
            }

            foreach (var l_SubDirectory in s_SubDirectories)
            {
                var l_DirectoryPath = $"{p_RemoteDirectoryPath}/{Path.GetFileName(l_SubDirectory)}";

                Console.WriteLine($"Path: {l_DirectoryPath}");

                CreateDirectory(l_DirectoryPath);

                UploadRecursive(l_SubDirectory, l_DirectoryPath);
            }
        }
    }
}
