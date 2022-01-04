using System;
using System.IO;
using FunDBLib.MetaData;

namespace FunDBLib
{
    public static class DataRecordParser
    {
        internal static void WriteRecord<TRecord>(FileStream fileStream, TableMetaData tableMetaData, DataRecord<TRecord> dataRecord)
            where TRecord : class, new()
        {
            WriteRecordAddress(fileStream, dataRecord);

            AddRowToTable(fileStream, tableMetaData, dataRecord.Record);
        }

        internal static void WriteRecordAddress<TRecord>(FileStream fileStream, DataRecord<TRecord> dataRecord)
            where TRecord : class, new()
        {
            byte[] prevAddressBytes = BinaryHelper.Serialize(dataRecord.PrevAddress);
            fileStream.Write(prevAddressBytes, 0, prevAddressBytes.Length);

            byte[] nextAddressBytes = BinaryHelper.Serialize(dataRecord.NextAddress);
            fileStream.Write(nextAddressBytes, 0, nextAddressBytes.Length);
        }

        private static void AddRowToTable<TRecord>(FileStream fileStream, TableMetaData tableMetaData, TRecord record)
            where TRecord : class, new()
        {
            byte[] rowBytes = new byte[0];

            foreach (var field in tableMetaData.Fields)
            {
                var fieldValue = field.Property.GetValue(record);
                if (field.FieldType == EnumFieldTypes.String && fieldValue != null)
                {
                    string fieldValueString = (string)fieldValue;
                    if (fieldValueString.Length > field.Length)
                        fieldValueString = fieldValueString.Substring(0, field.Length);
                    fieldValue = fieldValueString;
                }

                rowBytes = AddToRow(fieldValue, rowBytes);
            }

            fileStream.Write(rowBytes, 0, rowBytes.Length);
        }

        private static byte[] AddToRow(object fieldValue, byte[] rowBytes)
        {
            var fieldBytes = BinaryHelper.Serialize(fieldValue);
            byte[] fieldBytesLength = new byte[1] { (byte)fieldBytes.Length };
            byte[] newRow = new byte[rowBytes.Length + fieldBytes.Length + 1];
            rowBytes.CopyTo(newRow, 0);
            fieldBytesLength.CopyTo(newRow, rowBytes.Length);
            fieldBytes.CopyTo(newRow, rowBytes.Length + 1);

            return newRow;
        }

        internal static DataRecord<TRecord> ReadRecord<TRecord>(FileStream fileStream, TableMetaData tableMetaData)
            where TRecord : class, new()
        {
            byte[] prevAddressBytes = new byte[8];
            fileStream.Read(prevAddressBytes, 0, prevAddressBytes.Length);
            long prevAddress = BinaryHelper.DeserializeLong(prevAddressBytes);

            byte[] nextAddressBytes = new byte[8];
            fileStream.Read(nextAddressBytes, 0, nextAddressBytes.Length);
            long nextAddress = BinaryHelper.DeserializeLong(nextAddressBytes);

            TRecord record = new TRecord();

            foreach (var field in tableMetaData.Fields)
            {
                var fieldValue = ReadField(fileStream, field.FieldType);
                field.Property.SetValue(record, fieldValue);
            }

            return new DataRecord<TRecord>(prevAddress, nextAddress, record);
        }

        private static object ReadField(FileStream fileStream, EnumFieldTypes fieldType)
        {
            byte[] fieldLengthByte = new byte[1];
            fileStream.Read(fieldLengthByte, 0, fieldLengthByte.Length);

            byte[] fieldValueBytes = new byte[fieldLengthByte[0]];
            fileStream.Read(fieldValueBytes, 0, fieldValueBytes.Length);

            return Deserialize(fieldType, fieldValueBytes);
        }

        private static object Deserialize(EnumFieldTypes fieldType, byte[] fieldBytes)
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
    }
}