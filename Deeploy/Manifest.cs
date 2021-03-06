﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Deeploy
{
    [DataContract]
    public class Manifest
    {
        [DataContract]
        public class Entry
        {
            [DataMember]
            public string Path { get; set; }
            [DataMember]
            public string Hash { get; set; }
            [DataMember]
            public long Size { get; set; }

            public override string ToString()
            {
                return Path;
            }
        }

        [DataMember]
        public uint Build { get; set; }
        [DataMember]
        public string Commit { get; set; }
        [DataMember]
        public string BaseUrl { get; set; }
        [DataMember]
        public Entry[] Entries { get; set; }

        public string Serialize()
        {
            var s_Serializer = new DataContractJsonSerializer(typeof(Manifest));
            var s_Json = string.Empty;

            try
            {
                using (var s_Stream = new MemoryStream())
                {
                    s_Serializer.WriteObject(s_Stream, this);

                    s_Stream.Position = 0;

                    s_Json = new StreamReader(s_Stream).ReadToEnd();
                }
            }
            catch (Exception p_Exception)
            {
                Debug.WriteLine($"Exception: {p_Exception.Message}");
            }


            return s_Json;
        }

        public bool Deserialize(string p_JsonData)
        {
            // Hold our incoming manifest
            Manifest s_Manifest = null;

            try
            {
                // Create our serializer and try to parse the manifest
                var s_Serializer = new DataContractJsonSerializer(typeof(Manifest));
                using (var s_Stream = new MemoryStream(Encoding.UTF8.GetBytes(p_JsonData)))
                    s_Manifest = (Manifest)s_Serializer.ReadObject(s_Stream);
            }
            catch (Exception p_Exception)
            {
                Debug.WriteLine("Exception: {0}", p_Exception.Message);
            }

            // See if we successfully got a manifest
            if (s_Manifest == null)
                return false;

            // Copy pasta
            Build = s_Manifest.Build;
            Commit = s_Manifest.Commit;
            BaseUrl = s_Manifest.BaseUrl;
            Entries = s_Manifest.Entries;

            return true;
        }
    }
}