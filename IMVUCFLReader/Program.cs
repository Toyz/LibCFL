using System;
using System.IO;
using System.Threading.Tasks;

namespace IMVUCFLReader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cfl = new LibCFL.CFLMaker("created.cfl");
            cfl.HeaderCompression(LibCFL.CFLLoader.CompressionType.None);
            cfl.Add("index.xml", File.ReadAllBytes("index.xml"), LibCFL.CFLLoader.CompressionType.None);
            cfl.Add("_contents.json", File.ReadAllBytes("_contents.json"), LibCFL.CFLLoader.CompressionType.None);
            cfl.Add("Lowerbody.tga", File.ReadAllBytes("Lowerbody.tga"), LibCFL.CFLLoader.CompressionType.None);
            cfl.Finish();

            var cflReader = new LibCFL.CFLLoader("created.cfl");
            var entires = await cflReader.GetEntries();
            foreach(var entry in entires)
            {
                Console.WriteLine($"{entry.Name}");
            }

            cfl.Dispose();
        }
    }
}
