using Deeploy.Deployment;
using System;

namespace Deeploy
{
    class Program
    {
        // deeploy -I<input dir> -O<output dir> -B<build number> -C<commit string> -U<base url>
        static int Main(string[] p_Args)
        {
            if (p_Args.Length < 1)
            {
                Console.WriteLine($"deeploy usage: deeploy -I<input directory> -O<output directory> -B<build number> -C<commit string> -U<base url> [-S]");
                Console.WriteLine("There is no space in between the flag and the start of the argument, user's must also escape spaces within quotes.");

                Console.WriteLine("-S enables sftp uploading, otherwise ftp will be used.");

                Console.WriteLine("Example: deeploy -I\"C:\\Input Folder\" -O\"C:\\Output Folder\" -B1337 -C\"abcdefghijklmnop\" -Uhttp://google.com/");
                return -1;
            }
            var s_Parser = new Parser();

            if (!s_Parser.Parse(p_Args))
            {
                Console.WriteLine("Error: Parsing arguments failed.");
                return -1; // FAIL
            }

            var s_Builder = new Builder();

            var s_BuildTask = s_Builder.GenerateUpdate(s_Parser.InputDirectory, s_Parser.OutputDirectory, s_Parser.UrlBase, s_Parser.BuildNumber, s_Parser.Commit);

            s_BuildTask.Wait();

            if (!s_BuildTask.Result)
            {
                Console.WriteLine("Error: Failed to build the update.");
                return -1;
            }

            Console.WriteLine("Update built successfully!");

            // If we are skipping deployment
            if (s_Parser.NoDeploy)
                return 0; // OK

            var s_UploadTask = (s_Parser.SftpUpload ? (Deployer)new SshDeployer() : new FtpDeployer()).Deploy(s_Builder.LastManifest, s_Parser.OutputDirectory);

            s_UploadTask.Wait();

            if (!s_UploadTask.Result)
            {
                Console.WriteLine("Error: Deploy failed.");
                return -1;
            }

            Console.WriteLine("Success: Deploy successful.");

            return 0; // OK
        }
    }
}
