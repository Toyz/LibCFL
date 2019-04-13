using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibCFL
{
    public class CFLMaker : IDisposable
    {
        private readonly CFLHeader.CflTypeFlag _cflType;
        private readonly string _file;
        private BinaryWriter _bin;
        private CFLHeader _cFlHeader;
        private List<CFLEntry> _entries;

        public CFLMaker(string file, CFLHeader.CflTypeFlag cflType = CFLHeader.CflTypeFlag.CFL3)
        {
            _file = file;
            _cflType = cflType;

            Init();
        }

        public void Dispose()
        {
            _bin?.Dispose();
        }

        private void Init()
        {
            _entries = new List<CFLEntry>();

            _bin = new BinaryWriter(File.Open(_file, FileMode.Create));

            _cFlHeader = new CFLHeader(_cflType);
        }

        public void HeaderCompression(CFLLoader.CompressionType compression)
        {
            /*
            if (compression == CFLLoader.CompressionType.LZMA)
                throw new NotSupportedException();
                */
            _cFlHeader.Compression = compression;
        }

        public void Add(string name, byte[] data, CFLLoader.CompressionType compression)
        {
            /*
            if (compression == CFLLoader.CompressionType.LZMA)
                throw new NotSupportedException();
*/
            var entry = new CFLEntry
            {
                Name = name,
                UnpackedSize = data.Length,
                Compression = compression,
                FileContents = data
            };

            if (_cFlHeader.SupportsHash)
            {
                entry.Hash = Helpers.CalculateMD5Hash(data);
            }

            _entries.Add(entry);
        }

        public void Finish()
        {
            _bin.BaseStream.Position = 12;
            foreach (var entry in _entries)
                using (var binFile = new BinaryWriter(new MemoryStream()))
                {
                    if (entry.Compression == CFLLoader.CompressionType.None)
                    {
                        binFile.Write(entry.FileContents.Length);
                        binFile.Write(entry.FileContents);
                    }
                    else
                    {
                        var cData = Helpers.Compress(entry.FileContents);
                        binFile.Write(cData.Length);
                        binFile.Write(cData);
                    }

                    var f = (MemoryStream) binFile.BaseStream;
                    var data = f.ToArray();

                    entry.Offset = (int) _bin.BaseStream.Position;
                    _bin.Write(data);
                }

            _cFlHeader.SeekPoint = (int) _bin.BaseStream.Position;

            _bin.Write((int) _cFlHeader.Compression);
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

                    if (_cFlHeader.SupportsHash)
                    {
                        var hash = entry.Hash.ToCharArray();
                        binEntry.Write(hash.Length);
                        binEntry.Write(hash);

                        dicSize += 4 + hash.Length;
                    }
                }

                var f = (MemoryStream) binEntry.BaseStream;
                if (_cFlHeader.Compression == CFLLoader.CompressionType.LZMA)
                {
                    var data = Helpers.Compress(f.ToArray());
                    dicSize = data.Length;
                    _bin.Write(dicSize);
                    _bin.Write(data);
                }
                else
                {
                    _bin.Write(dicSize);
                    _bin.Write(f.ToArray());
                }
            }

            _cFlHeader.CompressedDirectorySize = dicSize;

            var h = Encoding.UTF8.GetBytes(_cFlHeader.CflType.ToString());
            _bin.Seek(0, SeekOrigin.Begin);
            _bin.Write(h);
            _bin.Write(_cFlHeader.SeekPoint);
            _bin.Write(_cFlHeader.CompressedDirectorySize);

            _bin.Close();
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