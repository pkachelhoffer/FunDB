using System;
using System.IO;
using FunDBLib.MetaData;

namespace FunDBLib
{
    internal class DataRecord
    {
        public long PrevAddress { get; set; }
        public long NextAddress { get; set; }

        public DataRecord()
        {
            
        }

        public DataRecord(long prevAddress, long nextAddress)
        {
            PrevAddress = prevAddress;
            NextAddress = nextAddress;
        }
    }

    internal class DataRecord<TRecord> : DataRecord
        where TRecord : class, new()
        
    {
        public TRecord Row { get; private set; }

        internal DataRecord(long prevAddress, long nextAddress, TRecord row) : base(prevAddress, nextAddress)
        {
            Row = row;
        }
    }
}