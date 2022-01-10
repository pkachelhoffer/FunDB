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

        internal RecordReadTracker RecordReadTracker { get; set; }

        public FDTable()
        {
            RecordReadTracker = new RecordReadTracker();
        }

        internal void Initialise(string basePath)
        {
            string fileName = $"fdb_{GetTableName()}.dat";
            DataPath = Path.Combine(basePath, fileName);

            HeaderMetaData = new TableMetaData(typeof(HeaderData));

            if (!File.Exists(DataPath))
                CreateTable();

            ReadHeaderData();

            InitialiseTyped();
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

        protected abstract void InitialiseTyped();
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

        protected override sealed void InitialiseTyped()
        {
            if (TableMetaData.PrimaryKey != null)
                ProcessPrimaryKey();
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

        private void ProcessPrimaryKey()
        {
            var primaryKey = TableMetaData.FieldDictionary[TableMetaData.PrimaryKey];

            if (primaryKey.FieldType == EnumFieldTypes.Int)
                AddIndex("PK", s => 
                {
                    return new PrimaryKeyIndexInt() { PrimaryKey = (int)primaryKey.Property.GetValue(s)};
                });
            else if (primaryKey.FieldType == EnumFieldTypes.Long)
                AddIndex("PK", s => 
                {
                    return new PrimaryKeyIndexLong() { PrimaryKey = (long)primaryKey.Property.GetValue(s)};
                });
            else if (primaryKey.FieldType == EnumFieldTypes.String)
                AddIndex("PK", s => 
                {
                    return new PrimaryKeyIndexString() { PrimaryKey = (string)primaryKey.Property.GetValue(s)};
                });
            else if (primaryKey.FieldType == EnumFieldTypes.Byte)
                AddIndex("PK", s => 
                {
                    return new PrimaryKeyIndexByte() { PrimaryKey = (byte)primaryKey.Property.GetValue(s)};
                });
            else if (primaryKey.FieldType == EnumFieldTypes.Enum)
                AddIndex("PK", s => 
                {
                    return new PrimaryKeyIndexInt() { PrimaryKey = (int)primaryKey.Property.GetValue(s)};
                });
            else
                throw new Exception($"Primary key type {primaryKey.FieldType} not supported.");
        }

        public void AddIndex<TFDIndexDefinition>(string name, Func<TTableDefinition, TFDIndexDefinition> funcGenerateIndex)
            where TFDIndexDefinition : class, new()
        {
            var index = new FDIndex<TTableDefinition, TFDIndexDefinition>(funcGenerateIndex, DataPath, GetTableName(), name);

            Indexes.Add(index);
        }

        public void Add(TTableDefinition row)
        {
            RowActions.Add((row, new RowAction(EnumRowActionType.Add)));
        }

        public void Update(TTableDefinition row)
        {
            RowActions.Add((row, new RowAction(EnumRowActionType.Update)));
        }

        public void Delete(TTableDefinition row)
        {
            RowActions.Add((row, new RowAction(EnumRowActionType.Delete)));
        }

        public void Submit()
        {
            using (var fileStream = new FileStream(DataPath, FileMode.Open))
            {
                foreach (var rowAction in RowActions)
                {
                    long address = 0;

                    if (rowAction.RowAction.RowActionType == EnumRowActionType.Add)
                        InsertData(fileStream, rowAction.Row, out address);
                    else if (rowAction.RowAction.RowActionType == EnumRowActionType.Update)
                        UpdateData(fileStream, rowAction.Row, out address);
                    else if (rowAction.RowAction.RowActionType == EnumRowActionType.Delete)
                        DeleteData(fileStream, rowAction.Row, out address);

                    MaintainIndexes(rowAction, address);
                }
            }

            SaveHeaderData();

            RowActions.Clear();
        }

        private void DeleteData(FileStream fileStream, TTableDefinition row, out long address)
        {
            if (!RecordReadTracker.ContainsRecord(row))
                throw new Exception("Record is not tracked. Only records read from database may be updated.");

            fileStream.Position = RecordReadTracker.GetAddress(row);
            var deleteRecord = DataRecordParser.ReadRecord(fileStream);

            address = fileStream.Position;

            DataRecord prevRecord = null;
            DataRecord nextRecord = null;

            // Set previous record next address
            if (deleteRecord.PrevAddress > 0)
            {
                fileStream.Position = deleteRecord.PrevAddress;
                prevRecord = DataRecordParser.ReadRecord(fileStream);
                prevRecord.NextAddress = deleteRecord.NextAddress;
                fileStream.Position = deleteRecord.PrevAddress;
                DataRecordParser.WriteRecordAddress(fileStream, prevRecord);
            }

            // Set next record previous address
            if (deleteRecord.NextAddress > 0)
            {
                fileStream.Position = deleteRecord.NextAddress;
                nextRecord = DataRecordParser.ReadRecord(fileStream);
                nextRecord.PrevAddress = deleteRecord.PrevAddress;
                fileStream.Position = deleteRecord.NextAddress;
                DataRecordParser.WriteRecordAddress(fileStream, nextRecord);
            }

            if (prevRecord == null)
            {
                if (nextRecord != null)
                    HeaderData.FirstRecordPosition = deleteRecord.NextAddress;
                else
                    HeaderData.FirstRecordPosition = 0;
            }

            if (nextRecord == null)
            {
                if (prevRecord != null)
                    HeaderData.LastRecordPosition = deleteRecord.PrevAddress;
                else
                    HeaderData.LastRecordPosition = 0;
            }
        }

        private void UpdateData(FileStream fileStream, TTableDefinition row, out long address)
        {
            if (!RecordReadTracker.ContainsRecord(row))
                throw new Exception("Record is not tracked. Only records read from database may be updated.");

            address = fileStream.Position;

            fileStream.Position = RecordReadTracker.GetAddress(row);
            var record = DataRecordParser.ReadRecord(fileStream);
            fileStream.Position = RecordReadTracker.GetAddress(row);

            DataRecordParser.WriteRecord(fileStream, TableMetaData, new DataRecord<TTableDefinition>(record.PrevAddress, record.NextAddress, row));
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

        private void MaintainIndexes((TTableDefinition, RowAction) rowAction, long address)
        {
            foreach(var index in Indexes)
                index.MaintainIndex(rowAction.Item1, rowAction.Item2, address);
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