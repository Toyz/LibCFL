using System;
using System.IO;
using System.Text;

namespace LibCFL
{
    public class Helpers
    {
        public static byte[] Decompress(CFLLoader.CompressionType compType, byte[] ins)
        {
            if (compType == CFLLoader.CompressionType.None)
            {
                return ins;
            }

            SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
            MemoryStream input = new MemoryStream(ins);
            MemoryStream output = new MemoryStream();

            // Read the decoder properties
            byte[] properties = new byte[5];
            input.Read(properties, 0, 5);
            // Read in the decompress file size.
            byte[] fileLengthBytes = new byte[8];
            //input.Read(fileLengthBytes, 0, 8);
            long fileLength = -1;//BitConverter.ToInt64(fileLengthBytes, 0);
            coder.SetDecoderProperties(properties);
            coder.Code(input, output, input.Length, fileLength, null);
            output.Flush();
            output.Position = 0;
            return output.ToArray();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
