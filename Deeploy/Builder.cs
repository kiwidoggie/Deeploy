using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Deeploy
{
    public class Builder
    {
        public Manifest LastManifest { get; set; }
        public async Task<Manifest> GenerateManifest(string p_InputDirectory, uint p_Build, string p_Commit = "", string p_BaseUrl = "")
        {
            // Create a new manifest to hold all of our information
            var s_Manifest = new Manifest
            {
                BaseUrl = p_BaseUrl,
                Build = p_Build,
                Commit = p_Commit
            };

            // This will take some time on large directories, but should be fairly instant in any other regards
            var s_Files = await Task.Run(() => Directory.GetFiles(p_InputDirectory, "*.*", SearchOption.AllDirectories));
            var s_ManifestEntries = new List<Manifest.Entry>();

            foreach (var l_File in s_Files)
            {
                var l_FileInfo = new FileInfo(l_File);

                string l_Hash;

                // Ignore .files and .directories
                if (l_FileInfo.Name.StartsWith("."))
                    continue;

                using (var l_FileReader = new BinaryReader(new FileStream(l_File, FileMode.Open, FileAccess.Read)))
                {
                    var l_Data = l_FileReader.ReadBytes((int)l_FileReader.BaseStream.Length);
                    var l_ShaHash = new SHA1CryptoServiceProvider().ComputeHash(l_Data);
                    l_Hash = BitConverter.ToString(l_ShaHash).Replace("-", string.Empty);
                }

                var l_Entry = new Manifest.Entry
                {
                    Hash = l_Hash,
                    Path = l_File.Replace(p_InputDirectory, string.Empty),
                    Size = l_FileInfo.Length
                };

                s_ManifestEntries.Add(l_Entry);
            }

            s_Manifest.Entries = s_ManifestEntries.ToArray();

            return s_Manifest;
        }

        public async Task<bool> GenerateUpdate(string p_InputDirectory, string p_PackageDirectory, string p_BaseUrl, uint p_Build = 0, string p_Commit = "")
        {
            var s_Manifest = await GenerateManifest(p_InputDirectory, p_Build, p_Commit, p_BaseUrl);

            foreach (var l_Entry in s_Manifest.Entries)
            {
                var l_FilePath = Path.GetFullPath(p_InputDirectory + l_Entry.Path);
                var l_OutPath = Path.GetFullPath(p_PackageDirectory + l_Entry.Path);
                var l_OutDirectory = Path.GetDirectoryName(l_OutPath);
                if (l_OutDirectory == null)
                    continue;

                if (!Directory.Exists(l_OutDirectory))
                    Directory.CreateDirectory(l_OutDirectory);

                if (!File.Exists(l_FilePath))
                    continue;

                Console.WriteLine($"Compressing {l_Entry.Path}.");

                File.WriteAllBytes(l_OutPath, ZLib.Compress(l_FilePath));
            }

            var s_ManifestPath = Path.Combine(p_PackageDirectory, "manifest.json");

            File.WriteAllText(s_ManifestPath, s_Manifest.Serialize());

            LastManifest = s_Manifest;

            return true;
        }
    }
}
