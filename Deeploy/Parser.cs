using System;
using System.IO;

namespace Deeploy
{
    // deeploy -I<input dir> -O<output dir> -B<build number> -C<commit string> -U<base url>
    public class Parser
    {
        public string InputDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public uint BuildNumber { get; set; }
        public string Commit { get; set; }
        public string UrlBase { get; set; }
        public bool NoDeploy { get; set; }
        public bool SftpUpload { get; set; }

        public bool Parse(string[] p_Arguments)
        {
            foreach (var l_Argument in p_Arguments)
            {
                if (l_Argument.Length < 2)
                    return false;

                var s_Ret = false;
                if (l_Argument.StartsWith("-I"))
                    s_Ret = ParseInputDirectory(l_Argument);
                if (l_Argument.StartsWith("-O"))
                    s_Ret = ParseOutputDirectory(l_Argument);
                if (l_Argument.StartsWith("-B"))
                    s_Ret = ParseBuildNumber(l_Argument);
                if (l_Argument.StartsWith("-C"))
                    s_Ret = ParseCommitString(l_Argument);
                if (l_Argument.StartsWith("-U"))
                    s_Ret = ParseBaseUrl(l_Argument);
                if (l_Argument.StartsWith("-NoDeploy"))
                {
                    s_Ret = true;
                    NoDeploy = true;
                }

                // TODO: This needs to be fixed to get the cert path from here. -S"CertPath" and apply it as usual, right now this only pulls from the settings.
                if (l_Argument.StartsWith("-S"))
                {
                    s_Ret = true;
                    SftpUpload = true;
                }

                if (!s_Ret)
                    Console.WriteLine($"Error: Invalid argument '{l_Argument}'.");
            }

            // Safty checking
            if (string.IsNullOrWhiteSpace(InputDirectory))
            {
                Console.WriteLine("Error: Invalid Input Directory.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(OutputDirectory))
            {
                Console.WriteLine("Error: Invalid Output Directory.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(UrlBase))
            {
                Console.WriteLine("Error: Invalid URL Base.");
                return false;
            }

            // We don't care about build number or commit because they are needed for operations.
            return true;
        }

        protected bool ParseInputDirectory(string p_Input)
        {
            var s_Directory = p_Input.Substring(2);
            if (!Directory.Exists(s_Directory))
                return false;

            InputDirectory = s_Directory;

            return true;
        }

        protected bool ParseOutputDirectory(string p_Input)
        {
            var s_Directory = p_Input.Substring(2);
            if (!Directory.Exists(s_Directory))
                return false;

            OutputDirectory = s_Directory;

            return true;
        }

        protected bool ParseBuildNumber(string p_Input)
        {
            var s_BuildNumberString = p_Input.Substring(2);
            uint s_BuildNumber;
            var s_Ret = uint.TryParse(s_BuildNumberString, out s_BuildNumber);

            if (!s_Ret)
                return false;

            BuildNumber = s_BuildNumber;

            return true;
        }

        protected bool ParseCommitString(string p_Input)
        {
            var s_Commit = p_Input.Substring(2);

            Commit = s_Commit;

            return true;
        }

        protected bool ParseBaseUrl(string p_Input)
        {
            var s_Url = p_Input.Substring(2);

            if (!Uri.IsWellFormedUriString(s_Url, UriKind.RelativeOrAbsolute))
                return false;

            UrlBase = s_Url;

            return true;
        }
    }
}
