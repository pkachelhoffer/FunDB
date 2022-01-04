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

            var rowID = ReadField(FileStream, EnumFieldTypes.Long, out bool found);
            if (!found)
            {
                row = null;
                return false;
            }

            foreach (var field in Table.TableMetaData.Fields)
            {
                var fieldValue = ReadField(FileStream, field.FieldType, out found);
                if (found)
                {
                    field.Property.SetValue(row, fieldValue);
                    dataFound = true;
                }
                else
                {
                    dataFound = false;
                    row = null;
                    break;
                }
            }

            return dataFound;
        }

        private object ReadField(FileStream fileStream, EnumFieldTypes fieldType, out bool found)
        {
            found = false;

            byte[] fieldLengthByte = new byte[1];
            var bytesRead = FileStream.Read(fieldLengthByte, 0, fieldLengthByte.Length);
            if (bytesRead > 0)
            {
                found = true;

                byte[] fieldValueBytes = new byte[fieldLengthByte[0]];
                FileStream.Read(fieldValueBytes, 0, fieldValueBytes.Length);

                return Deserialize(fieldType, fieldValueBytes);
            }
            else
            {
                found = false;
                return null;
            }
        }

        private object Deserialize(EnumFieldTypes fieldType, byte[] fieldBytes)
        {
            if (fieldType == EnumFieldTypes.Int)
                return BinaryHelper.DeserializeInt(fieldBytes);
            else if (fieldType == EnumFieldTypes.Decimal)
                return BinaryHelper.DeserializeDecimal(fieldBytes);
            else if (fieldType == EnumFieldTypes.String)
                return BinaryHelper.DeserializeString(fieldBytes);
            else if (fieldType == EnumFieldTypes.Byte)
                return fieldBytes[0];
            else if (fieldType == EnumFieldTypes.Long)
                return BinaryHelper.DeserializeLong(fieldBytes);
            else
                throw new Exception($"Field type not implemented: {fieldType}");
        }

        public void Dispose()
        {
            FileStream.Dispose();
        }
    }
}