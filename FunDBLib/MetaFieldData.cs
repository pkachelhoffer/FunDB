using System;

namespace FunDBLib
{
    internal struct MetaFieldData
    {
        public byte Length { get; set; }

        public byte[] Contents { get; set; }

        public MetaFieldData(byte length, byte[] contents)
        {
            Length = length;
            Contents = contents;
        }
    }
}