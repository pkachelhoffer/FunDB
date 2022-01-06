using System;
using System.Collections.Generic;
using FunDBLib.Attributes;

namespace FunDBLib.MetaData
{
    internal class TableMetaData
    {
        private Dictionary<string, MetaField> FieldDictionary { get; set; }

        public IEnumerable<MetaField> Fields { get { return FieldDictionary.Values; } }

        private Type TableType { get; set; }

        public TableMetaData(Type tableType)
        {
            TableType = tableType;

            Parse(tableType);
        }

        private void Parse(Type tableType)
        {
            FieldDictionary = new Dictionary<string, MetaField>();

            foreach (var property in tableType.GetProperties())
            {
                var attributes = property.GetCustomAttributes(false);
                
                bool ignore = false;

                FDColumnTextAttribute columnText = null;
                if (property.PropertyType == typeof(string) || property.PropertyType == typeof(char))
                    columnText = new FDColumnTextAttribute();

                foreach (var attribute in attributes)
                {
                    if (attribute is FDIgnoreAttribute)
                    {
                        ignore = true;
                        break;
                    }

                    if (attribute is FDColumnTextAttribute)
                        columnText = attribute as FDColumnTextAttribute;
                }

                if (ignore)
                    continue;

                byte length = 0;
                if (columnText != null)
                    length = (byte)columnText.CharacterLength;

                MetaField metaField = new MetaField(property.Name, ParseType(property.PropertyType), property, length);
                FieldDictionary.Add(metaField.Name, metaField);
            }
        }

        private EnumFieldTypes ParseType(Type type)
        {
            if (type == typeof(int))
                return EnumFieldTypes.Int;
            else if (type == typeof(decimal))
                return EnumFieldTypes.Decimal;
            else if (type == typeof(string))
                return EnumFieldTypes.String;
            else if (type.IsEnum)
                return EnumFieldTypes.Enum;
            else if (type == typeof(byte))
                return EnumFieldTypes.Byte;
            else if (type == typeof(long))
                return EnumFieldTypes.Long;
            else
                throw new Exception($"Invalid field type: {type}");
        }
    }
}