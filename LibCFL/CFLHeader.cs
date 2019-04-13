using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibCFL
{
    public class CFLHeader
    {
        public string CFLType { get; set; }

        public CFLLoader.CompressionType Compression { get; set; }

        public byte[] EntryList { get; }

        public CFLHeader(BinaryReader bin)
        {
            CFLType = new string(bin.ReadChars(4));

            var seekPoint = bin.ReadUInt32();
            bin.BaseStream.Seek(seekPoint, SeekOrigin.Begin);

            Compression = (CFLLoader.CompressionType) bin.ReadInt32();
            var directoryCompressionSize = (int)bin.ReadUInt32();

            EntryList = Helpers.Decompress(Compression, bin.ReadBytes(directoryCompressionSize));
        }

        public BinaryReader EntryReader()
        {
            return new BinaryReader(new MemoryStream(EntryList));
        }
    }
}
