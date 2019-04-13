using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LibCFL
{
    public class CFLMaker : IDisposable
    {
        private readonly List<CFLEntry> _entries;

        private readonly string _file;

        private readonly BinaryWriter bin;

        private readonly CFLHeader _cFlHeader;

        public CFLMaker(string file)
        {
            _file = file;
            _entries = new List<CFLEntry>();

            bin = new BinaryWriter(File.Open(file, FileMode.Create));

            _cFlHeader = new CFLHeader();
        }

        public void HeaderCompression(CFLLoader.CompressionType compression)
        {
            if (compression == CFLLoader.CompressionType.LZMA)
                throw new NotSupportedException();

            _cFlHeader.Compression = compression;
        }

        public void Add(string name, byte[] data, CFLLoader.CompressionType compression)
        {
            if (compression == CFLLoader.CompressionType.LZMA)
                throw new NotSupportedException();

            var entry = new CFLEntry()
            {
                Name = name,
                UnpackedSize = data.Length,
                Compression = compression,
                FileContents = data
            };
            _entries.Add(entry);
        }

        public void Finish()
        {
            bin.BaseStream.Position = 12;
            foreach (var entry in _entries)
            {
                using (var binFile = new BinaryWriter(new MemoryStream()))
                {
                    binFile.Write(entry.FileContents.Length);
                    binFile.Write(entry.FileContents);

                    var f = (MemoryStream) binFile.BaseStream;
                    var data = f.ToArray();

                    entry.Offset = (int)bin.BaseStream.Position;
                    bin.Write(data);
                }
                
            }

            _cFlHeader.SeekPoint = (int)bin.BaseStream.Position;

            bin.Write((int)_cFlHeader.Compression);
            // unpackedSize, offset, compression, namelen = struct.unpack('<iiih', entryHeader)

            var dicSize = 0;
            using (var binEntry = new BinaryWriter(new MemoryStream()))
            {

                foreach (var entry in _entries)
                {
                    var name = entry.Name.ToCharArray();

                    binEntry.Write(entry.UnpackedSize);
                    binEntry.Write(entry.Offset);
                    binEntry.Write((int) entry.Compression);
                    binEntry.Write((short) name.Length);
                    binEntry.Write(name);

                    dicSize += 14 + name.Length;
                }

                var f = (MemoryStream)binEntry.BaseStream;
                if (_cFlHeader.Compression == CFLLoader.CompressionType.LZMA)
                {
                    var data = Helpers.Compress(f.ToArray());
                    dicSize = data.Length;
                    bin.Write(dicSize);
                    bin.Write(data);
                }
                else
                {
                    bin.Write(dicSize);
                    bin.Write(f.ToArray());
                }
            }

            _cFlHeader.CompressedDirectorySize = dicSize;

            var h = Encoding.UTF8.GetBytes("CFL3");
            bin.Seek(0, SeekOrigin.Begin);
            bin.Write(h);
            bin.Write(_cFlHeader.SeekPoint);
            bin.Write(_cFlHeader.CompressedDirectorySize);

            bin.Close();
        }

        public void Dispose()
        {
            bin?.Dispose();
        }

        /*
        From Python
        60000000c5020000040000000900696e6465782e786d6c
        9800000025030000040000000e005f636f6e74656e74732e6a736f6e
        9c0200000c000000040000000d004c6f776572626f64792e746761

        From Local
        600000000c000000000000000900696e6465782e786d6c
        9800000070000000000000000e005f636f6e74656e74732e6a736f6e
        9c0200000c010000000000000d004c6f776572626f64792e746761
        */
    }
}
