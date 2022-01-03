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
            row = new TTableDefinition();

            bool dataFound = false;

            foreach (var field in Table.TableMetaData.Fields)
            {
                byte[] fieldLengthByte = new byte[1];
                var bytesRead = FileStream.Read(fieldLengthByte, 0, fieldLengthByte.Length);
                if (bytesRead > 0)
                {
                    byte[] fieldValueBytes = new byte[fieldLengthByte[0]];
                    FileStream.Read(fieldValueBytes, 0, fieldValueBytes.Length);

                    var fieldValue = Deserialize(field, fieldValueBytes);
                    field.Property.SetValue(row, fieldValue);

                    dataFound = true;
                }
                else
                {
                    row = null;
                    dataFound = false;
                    break;
                }
            }

            return dataFound;
        }

        private object Deserialize(MetaData.MetaField field, byte[] fieldBytes)
        {
            if (field.FieldType == EnumFieldTypes.Int)
                return BinaryHelper.DeserializeInt(fieldBytes);
            else if (field.FieldType == EnumFieldTypes.Decimal)
                return BinaryHelper.DeserializeDecimal(fieldBytes);
            else if (field.FieldType == EnumFieldTypes.String)
                return BinaryHelper.DeserializeString(fieldBytes);
            else if (field.FieldType == EnumFieldTypes.Byte)
                return fieldBytes[0];
            else
                throw new Exception($"Field type not implemented: {field.FieldType}");
        }

        public void Dispose()
        {
            FileStream.Dispose();
        }
    }
}