using System.IO;
using System.Security.Cryptography;
using System.Text;
using SevenZip;
using Decoder = SevenZip.Compression.LZMA.Decoder;
using Encoder = SevenZip.Compression.LZMA.Encoder;

namespace LibCFL
{
    public static class Helpers
    {
        private static readonly int dictionary = 1 << 23;

        // static Int32 posStateBits = 2;
        // static  Int32 litContextBits = 3; // for normal files
        // UInt32 litContextBits = 0; // for 32-bit data
        // static  Int32 litPosBits = 0;
        // UInt32 litPosBits = 2; // for 32-bit data
        // static   Int32 algorithm = 2;
        // static    Int32 numFastBytes = 128;

        private static readonly bool eos = true;

        private static readonly CoderPropID[] propIDs =
        {
            CoderPropID.DictionarySize,
            CoderPropID.PosStateBits,
            CoderPropID.LitContextBits,
            CoderPropID.LitPosBits,
            CoderPropID.Algorithm,
            CoderPropID.NumFastBytes,
            CoderPropID.MatchFinder,
            CoderPropID.EndMarker
        };

        // these are the default properties, keeping it simple for now:
        private static readonly object[] properties =
        {
            dictionary,
            2,
            3,
            0,
            2,
            128,
            "bt4",
            eos
        };

        public static byte[] Decompress(CFLLoader.CompressionType compType, byte[] ins)
        {
            if (compType == CFLLoader.CompressionType.None) return ins;

            var coder = new Decoder();
            var input = new MemoryStream(ins);
            var output = new MemoryStream();

            // Read the decoder properties
            var properties = new byte[5];
            input.Read(properties, 0, 5);
            // Read in the decompress file size.
            var fileLengthBytes = new byte[8];
            //input.Read(fileLengthBytes, 0, 8);
            long fileLength = -1; //BitConverter.ToInt64(fileLengthBytes, 0);
            coder.SetDecoderProperties(properties);
            coder.Code(input, output, input.Length, fileLength, null);
            output.Flush();
            output.Position = 0;
            return output.ToArray();
        }

        public static byte[] Compress(byte[] inputBytes)
        {
            var inStream = new MemoryStream(inputBytes);
            var outStream = new MemoryStream();
            var encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);

            encoder.Code(inStream, outStream, inStream.Length, -1, null);
            return outStream.ToArray();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static string CalculateMD5Hash(byte[] inputBytes)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5.ComputeHash(inputBytes);

            return ByteArrayToString(hash);
        }
    }
}