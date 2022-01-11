using System;
using System.Collections.Generic;
using FunDBLib.Attributes;

namespace FunDBLib.MetaData
{
    internal class TableMetaData
    {
        public Dictionary<string, MetaField> FieldDictionary { get; set; }

        public IEnumerable<MetaField> Fields { get { return FieldDictionary.Values; } }

        private Type TableType { get; set; }

        public string PrimaryKey { get; private set; }

        private Dictionary<EnumFieldTypes, int> TypeLengthDictionary { get; set; }

        public TableMetaData(Type tableType)
        {
            TableType = tableType;

            InitialiseTypeLengths();

            Parse(tableType);
        }

        public void AddMetaField(MetaField metaField)
        {
            FieldDictionary.Add(metaField.Name, metaField);
        }

        private void InitialiseTypeLengths()
        {
            TypeLengthDictionary = new Dictionary<EnumFieldTypes, int>();

            TypeLengthDictionary.Add(EnumFieldTypes.Int, BinaryHelper.Serialize(default(int)).Length);
            TypeLengthDictionary.Add(EnumFieldTypes.Decimal, BinaryHelper.Serialize(default(decimal)).Length);
            TypeLengthDictionary.Add(EnumFieldTypes.Long, BinaryHelper.Serialize(default(long)).Length);
            TypeLengthDictionary.Add(EnumFieldTypes.Enum, BinaryHelper.Serialize(default(int)).Length);
            TypeLengthDictionary.Add(EnumFieldTypes.Byte, BinaryHelper.Serialize(default(byte)).Length);
        }

        private void Parse(Type tableType)
        {
            FieldDictionary = new Dictionary<string, MetaField>();

            foreach (var property in tableType.GetProperties())
            {
                var attributes = property.GetCustomAttributes(false);
                
                bool ignore = false;

                FDColumnTextAttribute columnText = null;
                if (property.PropertyType == typeof(string))
                    columnText = new FDColumnTextAttribute();

                foreach (var attribute in attributes)
                {
                    if (attribute is FDIgnoreAttribute)
                    {
                        ignore = true;
                        break;
                    }

                    if (attribute is FDColumnTextAttribute textAttribute)
                        columnText = textAttribute;

                    if (attribute is FDPrimaryKeyAttribute pkAttribute)
                        PrimaryKey = property.Name;
                }

                if (ignore)
                    continue;

                var fieldType = ParseType(property.PropertyType);

                int length = 0;
                if (columnText != null)
                    length = (columnText.CharacterLength * 2) + 4; // Two bytes per character plus 4 bytes for length
                else if (TypeLengthDictionary.ContainsKey(fieldType))
                    length = TypeLengthDictionary[fieldType];
                else
                    throw new Exception($"Invalid field type: {fieldType}");
                    
                MetaField metaField = new MetaField(property.Name, fieldType, property, length);
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