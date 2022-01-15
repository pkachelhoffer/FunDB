using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FunDBLib.MetaData;

namespace FunDBLib.Index
{
    internal abstract class FDIndex<TTableDefinition>
    {
        internal abstract void MaintainIndex(TTableDefinition tableRow, RowAction rowAction, long rowAddress, FileStream fileStream);

        internal abstract void MaintainIndex(IEnumerable<IndexMaintainInstruction<TTableDefinition>> maintainInstructions, FileStream fileStream);

        internal abstract Type IndexDefinitionType { get; }

        public string DataPath { get; protected set; }
    }

    internal class FDIndex<TTableDefinition, TIndexDefinition> : FDIndex<TTableDefinition>
        where TIndexDefinition : class, IComparable, new()
    {
        private string IndexName { get; set; }

        private string TableName { get; set; }

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

        internal override void MaintainIndex(IEnumerable<IndexMaintainInstruction<TTableDefinition>> maintainInstructions, FileStream fileStream)
        {
            MaintainIndexAdd(maintainInstructions.Where(s => s.RowAction.RowActionType == EnumRowActionType.Add), fileStream);
            //var addInstructions = maintainInstructions.Where(s => s.RowAction.RowActionType == EnumRowActionType.Add).Select(s => FuncGenerateIndex(s.Row)).OrderBy(s => s).ToList();
        }

        private void MaintainIndexAdd(IEnumerable<IndexMaintainInstruction<TTableDefinition>> maintainInstructions, FileStream fileStream)
        {
            if (!maintainInstructions.Any())
                return;

            List<byte> listBytes = new List<byte>();

            var orderedList = maintainInstructions.Select(s => new { s.Address, IndexRow = FuncGenerateIndex(s.Row) }).OrderBy(s => s.IndexRow).ToArray();

            int rowLength = GenerateIndexRow(orderedList[0].IndexRow).Length + 8;
            byte[] indexBytes = new byte[rowLength];

            Seek(fileStream, orderedList[0].IndexRow, out bool found);
            long startAddress = fileStream.Position;

            int x = 0;

            TIndexDefinition indexRow = null;

            while (x < orderedList.Length)
            {
                bool takeIndex = false;
                if (fileStream.Position == fileStream.Length)
                    takeIndex = true;
                else
                {
                    if (indexBytes == null)
                    {
                        fileStream.Read(indexBytes, 0, indexBytes.Length);
                        ReadAddress(fileStream);
                    }
                }

                if (takeIndex)
                {
                    var indexRowBytes = GenerateIndexRow(orderedList[x].IndexRow);
                    indexRowBytes = AddToRow(orderedList[x].Address, indexRowBytes);

                    listBytes.AddRange(indexRowBytes);

                    x++;
                }
            }

            var listBytesArray = listBytes.ToArray();
            fileStream.Position = startAddress;
            fileStream.Write(listBytesArray, 0, listBytesArray.Length);
        }

        internal override void MaintainIndex(TTableDefinition tableRow, RowAction rowAction, long rowAddress, FileStream fileStream)
        {
            var indexRow = FuncGenerateIndex(tableRow);

            var indexRowBytes = GenerateIndexRow(indexRow);
            var rowLength = indexRowBytes.Length + 8;

            Seek(fileStream, indexRow, out bool found);

            if (rowAction.RowActionType == EnumRowActionType.Add)
            {
                ExtendIndex(fileStream, rowLength);
                WriteToIndex(indexRow, rowAddress, fileStream);
            }
            else if (rowAction.RowActionType == EnumRowActionType.Delete)
            {
                if (found)
                    ShortenIndex(fileStream, rowLength);
            }
            else if (rowAction.RowActionType == EnumRowActionType.Update)
            {
                WriteToIndex(indexRow, rowAddress, fileStream);
            }
        }

        private void ExtendIndex(FileStream fileStream, int length)
        {
            long startPosition = fileStream.Position;

            fileStream.SetLength(fileStream.Length + length);

            Queue<byte[]> queue = new Queue<byte[]>();

            while (fileStream.Position < fileStream.Length)
            {
                long position = fileStream.Position;

                byte[] readBytes = new byte[length];
                var bytesRead = fileStream.Read(readBytes, 0, readBytes.Length);
                queue.Enqueue(readBytes);

                if (queue.Count > 1)
                {
                    var bytes = queue.Dequeue();
                    fileStream.Position = position;
                    fileStream.Write(bytes, 0, bytes.Length);
                }
            }

            fileStream.Position = startPosition;
        }

        private void ShortenIndex(FileStream fileStream, int length)
        {
            long startPosition = fileStream.Position;

            if (fileStream.Length <= (fileStream.Position + length))
                fileStream.SetLength(fileStream.Length - length);
            else
            {
                long position = fileStream.Position;

                byte[] readBytes = new byte[length];
                fileStream.Read(readBytes, 0, readBytes.Length);

                while (fileStream.Position < fileStream.Length)
                {
                    readBytes = new byte[length];
                    fileStream.Read(readBytes, 0, readBytes.Length);
                    long nextPosition = fileStream.Position;

                    fileStream.Position = position;
                    fileStream.Write(readBytes, 0, readBytes.Length);

                    position = fileStream.Position;

                    fileStream.Position = nextPosition;
                }

                fileStream.SetLength(fileStream.Length - length);
                fileStream.Position = startPosition;
            }
        }

        private void WriteToIndex(TIndexDefinition indexRow, long rowAddress, FileStream fileStream)
        {
            var indexRowBytes = GenerateIndexRow(indexRow);
            indexRowBytes = AddToRow(rowAddress, indexRowBytes);

            fileStream.Write(indexRowBytes, 0, indexRowBytes.Length);
        }

        internal long Seek(TIndexDefinition indexRow, out bool found)
        {
            using (var fileStream = new FileStream(DataPath, FileMode.Open))
            {
                Seek(fileStream, indexRow, out found);

                if (found)
                    return ReadAddress(fileStream);
                else
                    return 0;
            }
        }

        private void Seek(FileStream fileStream, TIndexDefinition indexRow, out bool found)
        {
            var indexRowBytes = GenerateIndexRow(indexRow);
            var rowLength = indexRowBytes.Length + 8;

            int numberOfRecords = GetNumberOfRecords(fileStream, rowLength);

            if (numberOfRecords == 0)
                found = false;
            else
                Seek(fileStream, indexRow, rowLength, 0, numberOfRecords - 1, numberOfRecords / 2, out found);
        }

        private long ReadAddress(FileStream fileStream)
        {
            var row = DataRecordParser.ReadRow<TIndexDefinition>(fileStream, MetaDataKey);
            var addressBytes = new byte[8];
            fileStream.Read(addressBytes, 0, addressBytes.Length);
            var address = BinaryHelper.DeserializeLong(addressBytes);
            return address;
        }

        private void Seek(FileStream fileStream, TIndexDefinition indexRow, int rowLength, int rangeStart, int rangeEnd, int record, out bool found)
        {
            found = false;

            long address = rowLength * record;
            fileStream.Position = address;
            var row = DataRecordParser.ReadRow<TIndexDefinition>(fileStream, MetaDataKey);

            var compareResult = indexRow.CompareObjects(row); // -1 then row is less than indexRow
            found = compareResult == 0;


            // Statement 1: If rangeStart equal rangeEnd then index was not found
            // Statement 2: If record equals rangeEnd and indexRow is still larger, that means indexRow was not found and is after the rangeEnd
            // Statement 3: It was found, stop
            if (rangeStart == rangeEnd || (record == rangeEnd && compareResult == 1) || found)
            {
                if (found || compareResult == -1)
                    fileStream.Position = address;
                else
                    fileStream.Position = address + rowLength;
            }
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