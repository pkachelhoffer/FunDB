using System;
using System.IO;

namespace FunDBLib.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
    public class FDColumnTextAttribute : Attribute
    {
        private const byte ConstDefaultLength = 250;

        public byte CharacterLength { get; set; }

        public FDColumnTextAttribute() : this(ConstDefaultLength)
        {
            
        }

        public FDColumnTextAttribute(byte characterLength)
        {
            CharacterLength = characterLength;
        }
    }
}