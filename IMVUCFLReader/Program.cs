using System;
using System.Threading.Tasks;

namespace IMVUCFLReader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cfl = new LibCFL.CFLLoader("test.cfl");
            var files = await cfl.GetEntries();

            foreach (var entry in files)
            {
                entry.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            }
        }
    }
}
