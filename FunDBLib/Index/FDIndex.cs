using System;
using System.IO;
using FunDBLib.MetaData;

namespace FunDBLib.Index
{
    public abstract class FDIndex<TTableDefinition>
    {
        internal abstract void MaintainIndex(TTableDefinition tableRow, RowAction rowAction, long rowAddress);

        internal abstract Type IndexDefinitionType { get; }
    }

    public class FDIndex<TTableDefinition, TIndexDefinition> : FDIndex<TTableDefinition>
        where TTableDefinition : class, new()
        where TIndexDefinition : class, new()
    {
        private string IndexName { get; set; }

        private string TableName { get; set; }

        private string DataPath { get; set; }

        private Func<TTableDefinition, TIndexDefinition> FuncGenerateIndex { get; set; }

        private TableMetaData MetaDataKey { get; set; }

        internal override Type IndexDefinitionType => GetType().GenericTypeArguments[1];

        internal FDIndex(Func<TTableDefinition, TIndexDefinition> funcGenerateIndex, string tablePath, string tableName, string indexName)
        {
            FuncGenerateIndex = funcGenerateIndex;
            IndexName = indexName;
            TableName = tableName;

            string basePath = Path.GetDirectoryName(tablePath);

            MetaDataKey = new TableMetaData(funcGenerateIndex.GetType().GenericTypeArguments[1]);

            Initialise(basePath);
        }

        private void Initialise(string dataPath)
        {
            DataPath = Path.Combine(dataPath, $"fdb_{TableName}_{IndexName}.idx");

            if (!File.Exists(DataPath))
                using (var fileStream = new FileStream(DataPath, FileMode.CreateNew))
                { }
        }

        internal override void MaintainIndex(TTableDefinition tableRow, RowAction rowAction, long rowAddress)
        {
            var indexRow = FuncGenerateIndex(tableRow);

            if (rowAction.RowActionType == EnumRowActionType.Add)
                MaintainIndexAdd(indexRow, rowAddress);
        }

        private void MaintainIndexAdd(TIndexDefinition indexRow, long rowAddress)
        {
            var indexRowBytes = GenerateIndexRow(indexRow);
            indexRowBytes = AddToRow(rowAddress, indexRowBytes);

            using (var fileStream = new FileStream(DataPath, FileMode.Append))
                fileStream.Write(indexRowBytes, 0, indexRowBytes.Length);
        }

        internal long Seek(TIndexDefinition indexRow, out bool found)
        {
            var indexRowBytes = GenerateIndexRow(indexRow);
            var rowLength = indexRowBytes.Length + 8;

            using (var fileStream = new FileStream(DataPath, FileMode.Open))
            {
                int numberOfRecords = GetNumberOfRecords(fileStream, rowLength);

                Seek(fileStream, indexRow, rowLength, 0, numberOfRecords - 1, numberOfRecords / 2, out found);

                if (found)
                {
                    var row = DataRecordParser.ReadRow<TIndexDefinition>(fileStream, MetaDataKey);
                    var addressBytes = new byte[8];
                    fileStream.Read(addressBytes, 0, addressBytes.Length);
                    var address = BinaryHelper.DeserializeLong(addressBytes);
                    return address;
                }
                else
                    return 0;
            }
        }

        private void Seek(FileStream fileStream, TIndexDefinition indexRow, int rowLength, int rangeStart, int rangeEnd, int record, out bool found)
        {
            found = false;

            long address = rowLength * record;
            fileStream.Position = address;
            var row = DataRecordParser.ReadRow<TIndexDefinition>(fileStream, MetaDataKey);

            var compareResult = indexRow.CompareObjects(row); // -1 then row is less than indexRow
            found = compareResult == 0;

            if (rangeStart == rangeEnd || found)
                fileStream.Position = address;
            else
            {
                if (compareResult == 1) // Too low, need to search higher
                {
                    if (rangeStart + 1 == rangeEnd) // The last step handle manually to avoid stack overflow (1 / 2 is 0)
                    {
                        rangeStart++;
                        record++;
                    }
                    else
                    {
                        rangeStart = record;
                        record = rangeStart + ((rangeEnd - rangeStart) / 2);
                    }
                }
                else // Too high, need to search lower
                {
                    rangeEnd = record;
                    record = rangeStart + ((rangeEnd - rangeStart) / 2);
                }
                Seek(fileStream, indexRow, rowLength, rangeStart, rangeEnd, record, out found);
            }
        }

        private byte[] ReadAddress(FileStream fileStream, long address, int rowLength)
        {
            fileStream.Position = address;
            var bytes = new byte[rowLength];
            fileStream.Read(bytes, 0, bytes.Length);
            return bytes;
        }

        private byte[] GenerateIndexRow(TIndexDefinition indexRow)
        {
            byte[] rowBytes = new byte[0];

            foreach (var indexProperty in typeof(TIndexDefinition).GetProperties())
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