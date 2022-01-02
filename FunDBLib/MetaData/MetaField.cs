using System;
using System.Reflection;
using FunDBLib.Attributes;

namespace FunDBLib.MetaData
{
    internal class MetaField 
    {
        public string Name { get; set; }

        public EnumFieldTypes FieldType { get; set; }

        [FDIgnore]
        public PropertyInfo Property { get; set; }

        public byte ByteLength { get; set; }
        
        public MetaField()
        {
            
        }

        public MetaField(string name, EnumFieldTypes fieldType, PropertyInfo property, byte byteLength)
        {
            Name = name;
            FieldType = fieldType;
            Property = property;
            ByteLength = byteLength;
        }
    }
}