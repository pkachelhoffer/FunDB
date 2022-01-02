using System;
using System.IO;

namespace FunDBLib
{
    public class FDDataReader<TTableDefinition> : IDisposable
        where TTableDefinition : class, new()
    {
        private FDTable Table { get; set; }

        private int Index { get; set; }

        private FileStream FileStream { get; set; }

        internal FDDataReader(FDTable table)
        {
            Table = table;

            FileStream = new FileStream(Table.DataPath, FileMode.Open);
        }

        public bool ReadLine(out TTableDefinition row)
        {
            byte[] rowBytes = new byte[Table.TableMetaData.RowLengthBytes];
            int byteCount = FileStream.Read(rowBytes, 0, rowBytes.Length);

            if (byteCount == 0)
                row = default(TTableDefinition);
            else
                row = Deserialize(rowBytes);

            return byteCount > 0;
        }

        private TTableDefinition Deserialize(byte[] row)
        {
            TTableDefinition tableRow = new TTableDefinition();

            int index = 0;

            foreach (var field in Table.TableMetaData.Fields)
            {
                var fieldBytes = row.FDCopyArray(index, field.ByteLength);

                if (field.FieldType == EnumFieldTypes.Int)
                    field.Property.SetValue(tableRow, BinaryHelper.DeserializeInt(fieldBytes));
                else if (field.FieldType == EnumFieldTypes.Decimal)
                    field.Property.SetValue(tableRow, BinaryHelper.DeserializeDecimal(fieldBytes));
                else if (field.FieldType == EnumFieldTypes.String)
                    field.Property.SetValue(tableRow, BinaryHelper.DeserializeString(fieldBytes));
                else if (field.FieldType == EnumFieldTypes.Byte)
                    field.Property.SetValue(tableRow, fieldBytes[0]);

                index += field.ByteLength;
            }

            return tableRow;
        }

        public void Dispose()
        {
            FileStream.Dispose();
        }
    }
}