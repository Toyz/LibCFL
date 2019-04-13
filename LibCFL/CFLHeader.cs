using System;
using System.IO;

namespace LibCFL
{
    public class CFLHeader
    {
        public enum CflTypeFlag
        {
            CFL3,
            DFL3
        }

        public CFLHeader(CflTypeFlag cflType)
        {
            CflType = cflType;
            SeekPoint = 0;
            CompressedDirectorySize = 0;
        }

        public CFLHeader(BinaryReader bin)
        {
            var CflTypeHeader = new string(bin.ReadChars(4));
            CflType = (CflTypeFlag) Enum.Parse(typeof(CflTypeFlag), CflTypeHeader);

            SeekPoint = (int) bin.ReadUInt32();
            bin.BaseStream.Seek(SeekPoint, SeekOrigin.Begin);

            Compression = (CFLLoader.CompressionType) bin.ReadInt32();
            CompressedDirectorySize = bin.ReadInt32();

            if (Compression == CFLLoader.CompressionType.LZMA)
                EntryList = Helpers.Decompress(Compression, bin.ReadBytes(CompressedDirectorySize));
            else
                EntryList = bin.ReadBytes(CompressedDirectorySize);
        }

        public CflTypeFlag CflType { get; set; }

        public bool SupportsHash => CflType == CflTypeFlag.DFL3;

        public int SeekPoint { get; set; }

        public int CompressedDirectorySize { get; set; }

        public CFLLoader.CompressionType Compression { get; set; }

        public byte[] EntryList { get; }

        public int EntryLength => EntryList.Length;

        public BinaryReader EntryReader()
        {
            return new BinaryReader(new MemoryStream(EntryList));
        }
    }
}