using System;
using System.Reflection;

namespace FunDBLib
{
    internal struct MetaField
    {
        public string Name { get; set; }

        public Type FieldType { get; set; }

        public PropertyInfo Property { get; set; }

        public byte ByteLength { get; set; }
        
        public MetaField(string name, Type fieldType, PropertyInfo property, byte byteLength)
        {
            Name = name;
            FieldType = fieldType;
            Property = property;
            ByteLength = byteLength;
        }
    }
}