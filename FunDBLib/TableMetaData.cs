using System;
using System.Collections.Generic;
using FunDBLib.Attributes;

namespace FunDBLib
{
    internal class TableMetaData
    {
        private Dictionary<string, MetaField> FieldDictionary { get; set; }

        public int RowLengthBytes { get; private set; }

        public IEnumerable<MetaField> Fields { get { return FieldDictionary.Values; } }

        public TableMetaData(Type tableType)
        {
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
                    byteLength = (byte)(columnText.CharacterLength * 2); // Two bytes per character
                else if (property.PropertyType == typeof(int))
                    byteLength = (byte)BitConverter.GetBytes(int.MaxValue).Length;
                else if (property.PropertyType == typeof(decimal))
                    byteLength = (byte)BinaryHelper.Serialize(decimal.MaxValue).Length;
                else
                    throw new Exception($"Field type {property.PropertyType} not supported. Use FDIgnore to exclude property.");

                MetaField metaField = new MetaField(property.Name, property.PropertyType, property, byteLength);
                FieldDictionary.Add(metaField.Name, metaField);

                RowLengthBytes += byteLength;
            }
        }
    }
}