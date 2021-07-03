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
        private TableMetaData TableMetaData { get; set; }

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

            using(var sr = new FileStream(DataPath, FileMode.Create))
                sr.Write(rowBytes, 0, rowBytes.Length);
        }

        private IEnumerable<TTableDefinition> Read()
        {
            return null;
        }
    }
}