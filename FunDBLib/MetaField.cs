using System;
using System.Reflection;

namespace FunDBLib
{
    internal struct MetaField
    {
        public string Name { get; set; }

        public Type FieldType { get; set; }

        public PropertyInfo Property { get; set; }
        
        public MetaField(string name, Type fieldType, PropertyInfo property)
        {
            Name = name;
            FieldType = fieldType;
            Property = property;
        }
    }
}