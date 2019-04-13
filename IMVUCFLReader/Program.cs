using System;
using System.IO;
using System.Threading.Tasks;

namespace IMVUCFLReader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cfl = new LibCFL.CFLMaker("created.cfl", LibCFL.CFLHeader.CflTypeFlag.DFL3);
            cfl.HeaderCompression(LibCFL.CFLLoader.CompressionType.LZMA);
            cfl.Add("index.xml", File.ReadAllBytes("index.xml"), LibCFL.CFLLoader.CompressionType.LZMA);
            cfl.Add("_contents.json", File.ReadAllBytes("_contents.json"), LibCFL.CFLLoader.CompressionType.LZMA);
            cfl.Add("Lowerbody.tga", File.ReadAllBytes("Lowerbody.tga"), LibCFL.CFLLoader.CompressionType.LZMA);
            cfl.Finish();

            var cflReader = new LibCFL.CFLLoader("created.cfl");
            var entires = await cflReader.GetEntries();
            foreach(var entry in entires)
            {
                Console.WriteLine($"{entry.Name} -> {entry.Hash}");
            }
        }
    }
}
