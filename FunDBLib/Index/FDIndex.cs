using System.IO;

namespace FunDBLib.Index
{
    public class FDIndex
    {
        private string DataPath { get; set; }

        private string IndexName { get; set; }

        private string TableName { get; set; }

        private string TableKey { get; set; }

        private object IndexDefinition { get; set; }

        public FDIndex(string tableName, string indexName, string tableKey, object indexDefinition)
        {
            TableName = tableName;
            IndexName = indexName;
            TableKey = tableKey;
            IndexDefinition = indexDefinition;
        }

        private void Initialise(string basePath)
        {
            DataPath = Path.Combine(basePath, $"{TableName}_{IndexName}.idx");
            if (!File.Exists(DataPath))
                File.Create(DataPath);
        }

        public void MaintainIndex(object tableRow, long rowAddress)
        {
            var indexRowBytes = GenerateIndexRow(tableRow);
            indexRowBytes = AddToRow(rowAddress, indexRowBytes);

            using (var fileStream = new FileStream(DataPath, FileMode.Append))
                fileStream.Write(indexRowBytes, 0, indexRowBytes.Length);
        }

        private byte[] GenerateIndexRow(object tableRow)
        {
            var tableType = tableRow.GetType();

            byte[] rowBytes = new byte[0];

            foreach (var indexProperty in IndexDefinition.GetType().GetProperties())
            {
                var tableProperty = tableType.GetProperty(indexProperty.Name);
                if (tableProperty == null)
                    continue;
                
                var indexValue = tableProperty.GetValue(tableRow);

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