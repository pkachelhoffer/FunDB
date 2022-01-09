using System;
using System.Collections.Generic;

namespace FunDBLib
{
    public class RecordReadTracker
    {
        private Dictionary<object, long> RecordDictionary { get; set; }

        public RecordReadTracker()
        {
            RecordDictionary = new Dictionary<object, long>();
        }

        public void Add(object record, long address)
        {
            if (!RecordDictionary.ContainsKey(record))
                RecordDictionary.Add(record, address);
        }

        public bool ContainsRecord(object record)
        {
            return RecordDictionary.ContainsKey(record);
        }

        public long GetAddress(object record)
        {
            return RecordDictionary[record];
        }
    }
}