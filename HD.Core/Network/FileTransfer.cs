using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using ProtoBuf;

namespace HD
{
    [ProtoContract]
    public class FileTransfer
    {
        [ProtoMember(1, IsRequired = true)]
        public string Filename { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public byte[] Data { get; set; }

        public static FileTransfer Load(string filename)
        {
            var result = new FileTransfer() { Filename = Path.GetFileName(filename) };
#if WINDOWS
            result.Data = File.ReadAllBytes(filename);
#endif
            return result;
        }

        public string Save(string serverAddress, int port)
        {
            Directory.CreateDirectory(Path.Combine(Utility.SavePath, serverAddress + "-" + port));
            var filename = Path.Combine(Utility.SavePath, Path.Combine(serverAddress + "-" + port, Filename));
#if WINDOWS
            File.WriteAllBytes(filename, Data);
#endif
            return filename;
        }
    }
}