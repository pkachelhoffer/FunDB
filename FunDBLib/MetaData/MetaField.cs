using System;
using System.Reflection;
using FunDBLib.Attributes;

namespace FunDBLib.MetaData
{
    internal class MetaField 
    {
        public string Name { get; set; }

        public EnumFieldTypes FieldType { get; set; }

        public PropertyInfo Property { get; set; }

        public int Length { get; set; }
        
        public MetaField()
        {
            
        }

        public MetaField(string name, EnumFieldTypes fieldType, PropertyInfo property, int length)
        {
            Name = name;
            FieldType = fieldType;
            Property = property;
            Length = length;
        }
    }
}