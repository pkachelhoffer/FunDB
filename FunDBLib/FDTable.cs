using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using FunDBLib.Attributes;
using FunDBLib.Index;
using FunDBLib.MetaData;

namespace FunDBLib
{
    public abstract class FDTable
    {
        internal string DataPath { get; private set; }

        internal TableMetaData TableMetaData { get; set; }

        internal HeaderData HeaderData { get; set; }
        private TableMetaData HeaderMetaData { get; set; }

        internal void Initialise(string basePath)
        {
            string fileName = $"fdb_{GetTableName()}.dat";
            DataPath = Path.Combine(basePath, fileName);

            HeaderMetaData = new TableMetaData(typeof(HeaderData));

            if (!File.Exists(DataPath))
                CreateTable();

            ReadHeaderData();
        }

        private void CreateTable()
        {
            using (var fileStream = new FileStream(DataPath, FileMode.CreateNew))
                DataRecordParser.WriteRow(fileStream, HeaderMetaData, new HeaderData());
        }

        protected void ReadHeaderData()
        {
            using (var fileStream = new FileStream(DataPath, FileMode.Open))
                HeaderData = DataRecordParser.ReadRow<HeaderData>(fileStream, HeaderMetaData);
        }

        protected void SaveHeaderData()
        {
            using (var fileStream = new FileStream(DataPath, FileMode.Open))
                DataRecordParser.WriteRow(fileStream, HeaderMetaData, HeaderData);
        }

        internal abstract string GetTableName();
    }

    public class FDTable<TTableDefinition> : FDTable
        where TTableDefinition : class, new()
    {
        private Type RowType { get; set; }

        private List<(TTableDefinition Row, RowAction RowAction)> RowActions { get; set; }

        private List<FDIndex<TTableDefinition>> Indexes { get; set; }

        public FDTable()
        {
            RowActions = new List<(TTableDefinition, RowAction)>();

            RowType = typeof(TTableDefinition);

            TableMetaData = new TableMetaData(typeof(TTableDefinition));

            Indexes = new List<FDIndex<TTableDefinition>>();
        }

        internal override string GetTableName()
        {
            var definitionType = typeof(TTableDefinition);

            string tableName = definitionType.Name;

            var defintionAttributes = definitionType.GetCustomAttributes(true);
            foreach (var customAttribute in defintionAttributes)
                if (customAttribute is FDTableAttribute)
                    tableName = (customAttribute as FDTableAttribute).TableName;

            return tableName;
        }

        public void AddIndex<TFDIndexDefinition>(string name, Func<TTableDefinition, TFDIndexDefinition> funcGenerateIndex)
        {
            var index = new FDIndex<TTableDefinition, TFDIndexDefinition>(funcGenerateIndex, DataPath, GetTableName(), name);

            Indexes.Add(index);
        }

        public void Add(TTableDefinition row)
        {
            RowActions.Add((row, new RowAction(EnumRowActionType.Add)));
        }

        public void Submit()
        {
            using (var fileStream = new FileStream(DataPath, FileMode.Open))
            {
                foreach (var rowAction in RowActions)
                {
                    if (rowAction.RowAction.RowActionType == EnumRowActionType.Add)
                    {
                        InsertData(fileStream, rowAction.Row, out long address);
                        MaintainIndexes(rowAction.Row, address);
                    }
                }
            }

            SaveHeaderData();
        }

        private void InsertData(FileStream fileStream, TTableDefinition row, out long address)
        {
            var dataRecord = new DataRecord<TTableDefinition>(0, 0, row);

            fileStream.Seek(0, SeekOrigin.End);

            if (HeaderData.FirstRecordPosition == 0)
                HeaderData.FirstRecordPosition = fileStream.Position;
            else
            {
                long nextAddress = fileStream.Position;
                long prevAddress = HeaderData.LastRecordPosition;

                UpdatePreviousRecordNextAddress(fileStream, prevAddress, nextAddress);

                dataRecord.PrevAddress = HeaderData.LastRecordPosition;

                fileStream.Position = nextAddress;
            }

            HeaderData.LastRecordPosition = fileStream.Position;
            address = fileStream.Position;

            DataRecordParser.WriteRecord(fileStream, TableMetaData, dataRecord);
        }

        private void MaintainIndexes(TTableDefinition row, long address)
        {
            foreach(var index in Indexes)
            {
                index.MaintainIndex(row, address);
            }
        }

        private void UpdatePreviousRecordNextAddress(FileStream fileStream, long prevAddress, long nextAddress)
        {
            fileStream.Position = prevAddress;
            var prevRecord = DataRecordParser.ReadRecord(fileStream);
            prevRecord.NextAddress = nextAddress;
            fileStream.Position = prevAddress;
            DataRecordParser.WriteRecordAddress(fileStream, prevRecord);
        }

        public FDDataReader<TTableDefinition> GetReader()
        {
            ReadHeaderData();
            return new FDDataReader<TTableDefinition>(this);
        }
    }
}