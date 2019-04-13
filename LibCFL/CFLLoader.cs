﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LibCFL
{
    public class CFLLoader
    {
        private readonly string _file;

        public CFLLoader(string file)
        {
            _file = file;
        }

        public async Task<List<CFLEntry>> GetEntries()
        {
            return await Task.Run(getCflEntries);
        }

        private List<CFLEntry> getCflEntries()
        {
            var s = File.Open(_file, FileMode.Open);
            var bin = new BinaryReader(s);

            var header = new CFLHeader(bin);
            var ehb = header.EntryReader();

            var e = new List<CFLEntry>();
            var currentReadLength = 0;
            while (header.EntryList.Length != currentReadLength)
            {
                var entry = new CFLEntry(ehb, bin);
                e.Add(entry);

                currentReadLength += entry.EntrySize;
            }

            ehb.Close();

            bin.Close();

            return e;
        }

        public enum CompressionType
        {
            None = 0,
            LZMA = 4
        } 
    }
}
