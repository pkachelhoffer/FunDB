using System;
using System.IO;

namespace FunDBLib.Index
{
    public abstract class FDIndex<TFDTableDefinition>
    {
        internal abstract void MaintainIndex(TFDTableDefinition tableRow, long rowAddress);
    }

    public class FDIndex<TFDTableDefinition, TFDIndexDefinition> : FDIndex<TFDTableDefinition>
        where TFDTableDefinition : class, new()
    {
        private string IndexName { get; set; }

        private string TableName { get; set; }

        private string DataPath { get; set; }

        private Func<TFDTableDefinition, TFDIndexDefinition> FuncGenerateIndex { get; set; }

        internal FDIndex(Func<TFDTableDefinition, TFDIndexDefinition> funcGenerateIndex, string tablePath, string tableName, string indexName)
        {
            FuncGenerateIndex = funcGenerateIndex;
            IndexName = indexName;
            TableName = tableName;

            string basePath = Path.GetDirectoryName(tablePath);

            Initialise(basePath);
        }

        private void Initialise(string dataPath)
        {
            DataPath = Path.Combine(dataPath, $"{TableName}_{IndexName}.idx");
            
            if (!File.Exists(DataPath))
                using (var fileStream = new FileStream(DataPath, FileMode.CreateNew))
                { }
        }

        internal override void MaintainIndex(TFDTableDefinition tableRow, long rowAddress)
        {
            var indexRow = FuncGenerateIndex(tableRow);

            var indexRowBytes = GenerateIndexRow(indexRow);
            indexRowBytes = AddToRow(rowAddress, indexRowBytes);

            using (var fileStream = new FileStream(DataPath, FileMode.Append))
                fileStream.Write(indexRowBytes, 0, indexRowBytes.Length);
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

    }
}