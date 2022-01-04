using System;
using System.Collections.Generic;
using System.IO;
using FunDBLib.Attributes;
using FunDBLib.MetaData;

namespace FunDBLib
{
    public abstract class FDTable
    {
        protected long Counter { get; set; }

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
            long lastRecordAddress = 0;
            long nextRecordAddress = 0;
            DataRecord<TTableDefinition> prevRecord = null;

            using (var fileStream = new FileStream(DataPath, FileMode.Append))
            {
                foreach (var rowAction in RowActions)
                {
                    if (rowAction.RowAction.RowActionType == EnumRowActionType.Add)
                    {
                        nextRecordAddress = fileStream.Position;
                        if (prevRecord != null)
                        {
                            fileStream.Position = lastRecordAddress;
                            prevRecord.NextAddress = nextRecordAddress;
                            DataRecordParser.WriteRecordAddress(fileStream, prevRecord);
                            fileStream.Position = nextRecordAddress;
                        }
                        
                        var record = new DataRecord<TTableDefinition>(lastRecordAddress, 0, rowAction.Row);
                        lastRecordAddress = fileStream.Position;
                        DataRecordParser.WriteRecord(fileStream, TableMetaData, record);

                        prevRecord = record;
                    }
                }
            }
        }

        private void Seek(FileStream fileStream, long address)
        {
            fileStream.Position = address;
        }

        public FDDataReader<TTableDefinition> GetReader()
        {
            return new FDDataReader<TTableDefinition>(this);
        }
    }
}