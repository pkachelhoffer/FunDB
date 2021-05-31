using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunDB
{
    public struct BinaryField
    {
        public byte Length { get; set; }
        public byte[] Contents { get; set; }

        public BinaryField(byte length, byte[] contents)
        {
            Length = length;
            Contents = contents;
        }
    }
}
