using System;
using System.IO;
using FunDBLib.MetaData;

namespace FunDBLib
{
    internal class DataRecord<TRecord>
        where TRecord : class, new()
        
    {
        public long PrevAddress { get; set; }
        public long NextAddress { get; set; }

        public TRecord Record { get; private set; }

        internal DataRecord(long prevAddress, long nextAddress, TRecord record)
        {
            PrevAddress = prevAddress;
            NextAddress = nextAddress;
            Record = record;
        }
    }
}