using System;
using System.Collections.Generic;
using System.IO;

namespace FunDBLib.Index
{
    internal class IndexCollectionReader<TTableDefinition> : IDisposable
    {
        private IEnumerable<FDIndex<TTableDefinition>> Indexes { get; set; }

        private Dictionary<FDIndex<TTableDefinition>, FileStream> FileStreamDictionary { get; set; }

        public IndexCollectionReader(IEnumerable<FDIndex<TTableDefinition>> indexes)
        {
            Indexes = indexes;
            FileStreamDictionary = new Dictionary<FDIndex<TTableDefinition>, FileStream>();
        }

        public void MaintainIndexes(TTableDefinition tableRow, RowAction rowAction, long address)
        {
            foreach(var index in Indexes)
            {
                if (!FileStreamDictionary.ContainsKey(index))
                {
                    FileStream fileStream = new FileStream(index.DataPath, FileMode.Open);
                    FileStreamDictionary.Add(index, fileStream);
                }

                index.MaintainIndex(tableRow, rowAction, address, FileStreamDictionary[index]);
            }
        }

        public void Dispose()
        {
            foreach(var item in FileStreamDictionary)
                item.Value.Dispose();
        }
    }
}