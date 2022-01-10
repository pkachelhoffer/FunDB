using System;
using System.IO;
using FunDBLib.MetaData;

namespace FunDBLib.Index
{
    public abstract class FDIndex<TFDTableDefinition>
    {
        internal abstract void MaintainIndex(TFDTableDefinition tableRow, RowAction rowAction, long rowAddress);
    }

    public class FDIndex<TFDTableDefinition, TFDIndexDefinition> : FDIndex<TFDTableDefinition>
        where TFDTableDefinition : class, new()
        where TFDIndexDefinition : class, new()
    {
        private string IndexName { get; set; }

        private string TableName { get; set; }

        private string DataPath { get; set; }

        private Func<TFDTableDefinition, TFDIndexDefinition> FuncGenerateIndex { get; set; }

        private TableMetaData MetaData { get; set; }

        internal FDIndex(Func<TFDTableDefinition, TFDIndexDefinition> funcGenerateIndex, string tablePath, string tableName, string indexName)
        {
            FuncGenerateIndex = funcGenerateIndex;
            IndexName = indexName;
            TableName = tableName;

            string basePath = Path.GetDirectoryName(tablePath);

            MetaData = new TableMetaData(funcGenerateIndex.GetType().GenericTypeArguments[1]);

            Initialise(basePath);
        }

        private void Initialise(string dataPath)
        {
            DataPath = Path.Combine(dataPath, $"{TableName}_{IndexName}.idx");
            
            if (!File.Exists(DataPath))
                using (var fileStream = new FileStream(DataPath, FileMode.CreateNew))
                { }
        }

        internal override void MaintainIndex(TFDTableDefinition tableRow, RowAction rowAction, long rowAddress)
        {
            var indexRow = FuncGenerateIndex(tableRow);

            if (rowAction.RowActionType == EnumRowActionType.Add)
                MaintainIndexAdd(indexRow, rowAddress);
        }

        private void MaintainIndexAdd(TFDIndexDefinition indexRow, long rowAddress)
        {
            var indexRowBytes = GenerateIndexRow(indexRow);
            indexRowBytes = AddToRow(rowAddress, indexRowBytes);

            using (var fileStream = new FileStream(DataPath, FileMode.Append))
                fileStream.Write(indexRowBytes, 0, indexRowBytes.Length);

            Seek(indexRow);
        }

        private long Seek(TFDIndexDefinition indexRow)
        {
            var indexRowBytes = GenerateIndexRow(indexRow);
            var rowLength = indexRowBytes.Length + 8;

            using (var fileStream = new FileStream(DataPath, FileMode.Open))
            {
                int numberOfRecords = GetNumberOfRecords(fileStream, rowLength);

                bool done = Seek(fileStream, indexRow, rowLength, 0, numberOfRecords - 1, numberOfRecords / 2, out bool found);

                //var row = ReadAddress(fileStream, address, rowLength);
            }

            return 0;
        }

        private bool Seek(FileStream fileStream, TFDIndexDefinition indexRow, int rowLength, int rangeStart, int rangeEnd, int record, out bool found)
        {
            found = false;

            long address = rowLength * record;
            fileStream.Position = address;
            var row = DataRecordParser.ReadRow<TFDIndexDefinition>(fileStream, MetaData);

            var compareResult = indexRow.CompareObjects(row);
            found = compareResult == 0;

            if (rangeStart == rangeEnd || found)
            {
                fileStream.Position = address;
                return true;
            }
            else
            {
                return false;
            }
        }

        private byte[] ReadAddress(FileStream fileStream, long address, int rowLength)
        {
            fileStream.Position = address;
            var bytes = new byte[rowLength];
            fileStream.Read(bytes, 0, bytes.Length);
            return bytes;
        }

        private byte[] GenerateIndexRow(TFDIndexDefinition indexRow)
        {
            byte[] rowBytes = new byte[0];

            foreach (var indexProperty in typeof(TFDIndexDefinition).GetProperties())
            {
                var indexValue = indexProperty.GetValue(indexRow);

                rowBytes = AddToRow(indexValue, rowBytes);
            }

            return rowBytes;
        }

        private static byte[] AddToRow(object fieldValue, byte[] rowBytes)
        {
            var fieldBytes = BinaryHelper.Serialize(fieldValue);
            byte[] newRow = new byte[rowBytes.Length + fieldBytes.Length];
            rowBytes.CopyTo(newRow, 0);
            fieldBytes.CopyTo(newRow, rowBytes.Length);

            return newRow;
        }

        private int GetNumberOfRecords(FileStream fileStream, int rowLength)
        {
            return (int)(fileStream.Length / rowLength);
        }
    }
}