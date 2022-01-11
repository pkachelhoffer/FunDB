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

            WriteRow(fileStream, tableMetaData, dataRecord.Row);
        }

        internal static void WriteRecordAddress(FileStream fileStream, DataRecord dataRecord)
        {
            byte[] prevAddressBytes = BinaryHelper.Serialize(dataRecord.PrevAddress);
            fileStream.Write(prevAddressBytes, 0, prevAddressBytes.Length);

            byte[] nextAddressBytes = BinaryHelper.Serialize(dataRecord.NextAddress);
            fileStream.Write(nextAddressBytes, 0, nextAddressBytes.Length);
        }

        internal static void WriteRow<TRecord>(FileStream fileStream, TableMetaData tableMetaData, TRecord record)
            where TRecord : class, new()
        {
            byte[] rowBytes = new byte[0];

            foreach (var field in tableMetaData.Fields)
                rowBytes = AddToRow(field.Property.GetValue(record), field.Length, rowBytes);

            fileStream.Write(rowBytes, 0, rowBytes.Length);
        }

        private static byte[] AddToRow(object fieldValue, int length, byte[] rowBytes)
        {
            var fieldBytes = BinaryHelper.Serialize(fieldValue);
            byte[] newRow = new byte[rowBytes.Length + length];
            rowBytes.CopyTo(newRow, 0);
            fieldBytes.CopyTo(newRow, rowBytes.Length);

            return newRow;
        }

        internal static DataRecord<TRecord> ReadRecord<TRecord>(FileStream fileStream, TableMetaData tableMetaData)
            where TRecord : class, new()
        {
            var dataRecord = ReadRecord(fileStream);

            var record = ReadRow<TRecord>(fileStream, tableMetaData);

            return new DataRecord<TRecord>(dataRecord.PrevAddress, dataRecord.NextAddress, record);
        }

        internal static DataRecord ReadRecord(FileStream fileStream)
        {
            byte[] prevAddressBytes = new byte[8];
            fileStream.Read(prevAddressBytes, 0, prevAddressBytes.Length);
            long prevAddress = BinaryHelper.DeserializeLong(prevAddressBytes);

            byte[] nextAddressBytes = new byte[8];
            fileStream.Read(nextAddressBytes, 0, nextAddressBytes.Length);
            long nextAddress = BinaryHelper.DeserializeLong(nextAddressBytes);

            return new DataRecord(prevAddress, nextAddress);
        }

        internal static TRecord ReadRow<TRecord>(FileStream fileStream, TableMetaData tableMetaData)
            where TRecord : class, new()
        {
            TRecord record = new TRecord();

            foreach (var field in tableMetaData.Fields)
            {
                var fieldValue = ReadField(fileStream, field.Length, field.FieldType);
                field.Property.SetValue(record, fieldValue);
            }

            return record;
        }

        private static object ReadField(FileStream fileStream, int length, EnumFieldTypes fieldType)
        {
            byte[] fieldValueBytes = new byte[length];
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