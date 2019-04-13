using System.IO;

namespace LibCFL
{
    public class CFLHeader
    {
        public string CFLType { get; set; }

        public int SeekPoint { get; set; }

        public int CompressedDirectorySize { get; set; }

        public CFLLoader.CompressionType Compression { get; set; }

        public byte[] EntryList { get; }

        public int EntryLength => EntryList.Length;

        public CFLHeader()
        {
            CFLType = "CFL3";
            SeekPoint = 0;
            CompressedDirectorySize = 0;
        }

        public CFLHeader(BinaryReader bin)
        {
            CFLType = new string(bin.ReadChars(4));

            SeekPoint = (int)bin.ReadUInt32();
            bin.BaseStream.Seek(SeekPoint, SeekOrigin.Begin);

            Compression = (CFLLoader.CompressionType) bin.ReadInt32();
            CompressedDirectorySize = bin.ReadInt32();

            if (Compression == CFLLoader.CompressionType.LZMA)
            {
                EntryList = Helpers.Decompress(Compression, bin.ReadBytes(CompressedDirectorySize));
            }
            else
            {
                EntryList = bin.ReadBytes(CompressedDirectorySize);
            }
        }

        public BinaryReader EntryReader()
        {
            return new BinaryReader(new MemoryStream(EntryList));
        }
    }
}
