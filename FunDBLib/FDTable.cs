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
            foreach (var rowAction in RowActions)
            {
                if (rowAction.RowAction.RowActionType == EnumRowActionType.Add)
                    AddRowToTable(rowAction.Row);
            }
        }

        private void AddRowToTable(TTableDefinition row)
        {
            byte[] rowBytes = new byte[TableMetaData.RowLengthBytes];
            int byteIndex = 0;

            foreach (var field in TableMetaData.Fields)
            {
                var fieldBytes = BinaryHelper.Serialize(field.Property.GetValue(row), field.ByteLength);
                fieldBytes.CopyTo(rowBytes, byteIndex);
                byteIndex += field.ByteLength;
            }

            using (var sr = new FileStream(DataPath, FileMode.Create))
                sr.Write(rowBytes, 0, rowBytes.Length);
        }

        public FDDataReader<TTableDefinition> GetReader()
        {
            return new FDDataReader<TTableDefinition>(this);
        }
    }
}