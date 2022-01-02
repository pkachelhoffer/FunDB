using System;
using System.Collections.Generic;
using FunDBLib.Attributes;

namespace FunDBLib.MetaData
{
    internal class TableMetaData
    {
        private Dictionary<string, MetaField> FieldDictionary { get; set; }

        public int RowLengthBytes { get; private set; }

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

            RowLengthBytes = 0;

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

                byte byteLength = 0;
                if (columnText != null)
                    byteLength = (byte)((columnText.CharacterLength * 2) + 4); // Two bytes per character plus 4 bytes for length
                else if (property.PropertyType == typeof(int))
                    byteLength = 4;
                else if (property.PropertyType == typeof(decimal))
                    byteLength = (byte)BinaryHelper.Serialize(decimal.MaxValue).Length;
                else if (property.PropertyType.IsEnum)
                    byteLength = 4;
                else if (property.PropertyType == typeof(byte))
                    byteLength = 1;
                else
                    throw new Exception($"Field type {property.PropertyType} not supported. Use FDIgnore to exclude property.");

                MetaField metaField = new MetaField(property.Name, ParseType(property.PropertyType), property, byteLength);
                FieldDictionary.Add(metaField.Name, metaField);

                RowLengthBytes += byteLength;
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
            else
                throw new Exception($"Invalid field type: {type}");
        }

        // private void SaveTableMetaData()
        // {
        //     MetaFieldTable metaTable = new MetaFieldTable(TableType);
        //     foreach (var field in FieldDictionary.Values)
        //         metaTable.Add(field);

        //     metaTable.Submit();
        // }
    }
}