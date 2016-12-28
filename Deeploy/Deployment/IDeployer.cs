using System.Threading.Tasks;

namespace Deeploy.Deployment
{
    interface IDeployer
    {
        void UploadRecursive(string p_LocalDirectoryPath, string p_RemoteDirectoryPath);

        bool UploadFile(string p_Path, byte[] p_Data);
        bool CreateDirectory(string p_Path);

        Task<bool> Deploy(Manifest p_Manifest, string p_LocalPath);
    }
}
