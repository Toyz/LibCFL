using System.IO;

namespace LibCFL
{
    // unpackedSize, offset, compression, namelen = struct.unpack('<iiih', entryHeader)

    public class CFLEntry
    {
        public CFLEntry()
        {
        }

        public CFLEntry(BinaryReader bin, BinaryReader baseFile)
        {
            UnpackedSize = bin.ReadInt32();
            Offset = bin.ReadInt32();
            Compression = (CFLLoader.CompressionType) bin.ReadInt32();

            Name = new string(bin.ReadChars(bin.ReadInt16()));

            baseFile.BaseStream.Seek(Offset, SeekOrigin.Begin);

            if (Compression == CFLLoader.CompressionType.LZMA)
                FileContents = Helpers.Decompress(Compression, baseFile.ReadBytes((int) baseFile.ReadUInt32()));
            else
                FileContents = baseFile.ReadBytes((int) baseFile.ReadUInt32());
        }

        public string Name { get; set; }
        public int UnpackedSize { get; set; }
        public int Offset { get; set; }
        public CFLLoader.CompressionType Compression { get; set; }
        public int EntrySize => 14 + Name.Length + (Hash.Length > 0 ? 4 + Hash.Length : 0);
        public short NameLen => (short) Name.Length;
        public string Hash { get; set; }

        public byte[] FileContents { get; set; }

        public void Save(string filePath)
        {
            File.WriteAllBytes(Path.Combine(filePath, Name), FileContents);
        }
    }
}