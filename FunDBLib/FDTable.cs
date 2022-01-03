using System;
using System.Collections.Generic;
using System.IO;
using FunDBLib.Attributes;
using FunDBLib.MetaData;

namespace FunDBLib
{
    public abstract class FDTable
    {
        internal string DataPath { get; private set; }

        internal TableMetaData TableMetaData { get; set; }

        internal void SetDataPath(string basePath)
        {
            string fileName = $"fdb_{GetTableName()}.dat";
            DataPath = Path.Combine(basePath, fileName);
        }

        protected abstract string GetTableName();

        internal abstract Type GetRowType();
    }

    public class FDTable<TTableDefinition> : FDTable
        where TTableDefinition : class, new()
    {
        private Type RowType { get; set; }

        private List<(TTableDefinition Row, RowAction RowAction)> RowActions { get; set; }

        public FDTable()
        {
            RowActions = new List<(TTableDefinition, RowAction)>();

            RowType = typeof(TTableDefinition);

            TableMetaData = new TableMetaData(typeof(TTableDefinition));
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

        internal override Type GetRowType()
        {
            return RowType;
        }

        public void Add(TTableDefinition row)
        {
            RowActions.Add((row, new RowAction(EnumRowActionType.Add)));
        }

        public void Submit()
        {
            using (var sr = new FileStream(DataPath, FileMode.Append))
            {
                foreach (var rowAction in RowActions)
                {
                    if (rowAction.RowAction.RowActionType == EnumRowActionType.Add)
                        AddRowToTable(rowAction.Row, sr);
                }
            }
        }

        private void AddRowToTable(TTableDefinition row, FileStream fileStream)
        {
            byte[] rowBytes = new byte[0];

            foreach (var field in TableMetaData.Fields)
            {
                var fieldValue = field.Property.GetValue(row);
                if (field.FieldType == EnumFieldTypes.String && fieldValue != null)
                {
                    string fieldValueString = (string)fieldValue;
                    if (fieldValueString.Length > field.Length)
                        fieldValueString = fieldValueString.Substring(0, field.Length);
                    fieldValue = fieldValueString;
                }

                var fieldBytes = BinaryHelper.Serialize(fieldValue);
                byte[] fieldBytesLength = new byte[1] { (byte)fieldBytes.Length };
                byte[] newRow = new byte[rowBytes.Length + fieldBytes.Length + 1];
                rowBytes.CopyTo(newRow, 0);
                fieldBytesLength.CopyTo(newRow, rowBytes.Length);
                fieldBytes.CopyTo(newRow, rowBytes.Length + 1);

                rowBytes = newRow;
            }

            fileStream.Write(rowBytes, 0, rowBytes.Length);
        }

        public FDDataReader<TTableDefinition> GetReader()
        {
            return new FDDataReader<TTableDefinition>(this);
        }
    }
}