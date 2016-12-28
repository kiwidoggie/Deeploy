using System.IO;
using System.Threading.Tasks;
using DeploySettings = Deeploy.Properties.Settings;

namespace Deeploy.Deployment
{
    public abstract class Deployer : IDeployer
    {
        protected string Server { get; set; }
        protected int Port { get; set; }
        protected string Username { get; set; }
        protected string CertPath { get; set; }
        protected string Password { get; set; }
        protected bool Wipe { get; set; }

        public abstract bool CreateDirectory(string p_Path);
        public abstract Task<bool> Deploy(Manifest p_Manifest, string p_LocalPath);
        public abstract bool UploadFile(string p_Path, byte[] p_Data);
        public abstract void UploadRecursive(string p_LocalDirectoryPath, string p_RemoteDirectoryPath);

        protected Deployer()
        {
            Server = DeploySettings.Default.Server;
            Port = DeploySettings.Default.Port;
            Username = DeploySettings.Default.Username;
            CertPath = DeploySettings.Default.CertPath;
            Password = DeploySettings.Default.Password;
            Wipe = DeploySettings.Default.WipeBeforeUpload;
    }
        
        internal string GetNormalizedPath(string p_Path)
        {
            return p_Path
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace("\\", "/").Trim();
        }
    }
}
