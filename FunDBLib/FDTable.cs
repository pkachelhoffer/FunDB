using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FunDBLib.Attributes;

namespace FunDBLib
{
    public abstract class FDTable
    {
        protected string DataPath { get; private set; }

        internal void SetDataPath(string basePath)
        {
            string fileName = $"fdb_{GetTableName()}.dat";
            DataPath = Path.Combine(basePath, fileName);
        }

        protected abstract string GetTableName();
    }

    public class FDTable<TTableDefinition> : FDTable
    {
        private Dictionary<string, MetaField> FieldDictionary { get; set; }

        private Type RowType { get; set; }

        private List<(TTableDefinition Row, RowAction RowAction)> RowActions { get; set; }

        public FDTable()
        {
            RowActions = new List<(TTableDefinition, RowAction)>();

            RowType = typeof(TTableDefinition);

            FieldDictionary = PopulateMetaFieldDictionary();
        }

        protected override string GetTableName()
        {
            var definitionType = typeof(TTableDefinition);

            string tableName = definitionType.Name;

            var defintionAttributes = definitionType.GetCustomAttributes(true);
            foreach (var customAttribute in defintionAttributes)
                if (customAttribute is FDTableAttribute)
                    tableName = (customAttribute as FDTableAttribute).TableName;

            return tableName;
        }

        public void Add(TTableDefinition row)
        {
            RowActions.Add((row, new RowAction(EnumRowActionType.Add)));
        }

        public void Submit()
        {
            foreach (var rowAction in RowActions)
            {
                if (rowAction.RowAction.RowActionType == EnumRowActionType.Add)
                    AddRowToTable(rowAction.Row);
            }
        }

        private void AddRowToTable(TTableDefinition row)
        {
            List<MetaFieldData> fieldDataEntries = new List<MetaFieldData>();

            foreach (var field in FieldDictionary)
            {
                var fieldData = BinaryHelper.Serialize(field.Value.Property.GetValue(row));
                fieldDataEntries.Add(fieldData);
            }

            var bytesValues = BinaryHelper.Serialize(fieldDataEntries.ToArray());

            using(var sr = new FileStream(DataPath, FileMode.Create))
            {
                byte[] length = BitConverter.GetBytes(bytesValues.Length);
                var bytesLine = new byte[length.Length + bytesValues.Length];

                length.CopyTo(bytesLine, 0);
                bytesValues.CopyTo(bytesLine, length.Length);

                sr.Write(bytesLine, 0, bytesLine.Length);
            }
        }

        private IEnumerable<TTableDefinition> Read()
        {
            return null;
        }

        private static Dictionary<string, MetaField> PopulateMetaFieldDictionary()
        {
            Dictionary<string, MetaField> fieldDictionary = new Dictionary<string, MetaField>();

            var definitionType = typeof(TTableDefinition);

            foreach (var property in definitionType.GetProperties())
            {
                var attributes = property.GetCustomAttributes(false);
                
                bool ignore = false;

                foreach (var attribute in attributes)
                    if (attribute is FDIgnoreAttribute)
                    {
                        ignore = true;
                        break;
                    }

                if (ignore)
                    continue;

                MetaField metaField = new MetaField(property.Name, property.PropertyType, property);
                fieldDictionary.Add(metaField.Name, metaField);
            }

            return fieldDictionary;
        }
    }
}