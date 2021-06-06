#nullable enable
using System;
using System.IO;
using System.Text;

namespace FunDBLib
{
    public static class BinaryHelper
    {
        public static int DeserializeInt(byte[] content)
        {
            using (var ms = new MemoryStream(content))
            using (var br = new BinaryReader(ms))
            {
                var bytes = br.ReadBytes(content.Length);
                return BitConverter.ToInt32(bytes);
            }
        }

        public static string DeserializeString(byte[] content)
        {
            return Encoding.UTF8.GetString(content);
        }

        public static decimal DeserializeDecimal(byte[] content)
        {
            int[] bits = new int[4];
            bits[0] = ((content[0] | (content[1] << 8)) | (content[2] << 0x10)) | (content[3] << 0x18); //lo
            bits[1] = ((content[4] | (content[5] << 8)) | (content[6] << 0x10)) | (content[7] << 0x18); //mid
            bits[2] = ((content[8] | (content[9] << 8)) | (content[10] << 0x10)) | (content[11] << 0x18); //hi
            bits[3] = ((content[12] | (content[13] << 8)) | (content[14] << 0x10)) | (content[15] << 0x18); //flags

            return new decimal(bits);
        }

        internal static MetaFieldData Serialize(object? valueNullable)
        {
            if (valueNullable == null)
                return new MetaFieldData(0, new byte[0]);
            else
            {
                var value = (object)valueNullable;

                if (value is int)
                    return Serialize((int)value);
                else if (value is string)
                    return Serialize((string)value);
                else if (value is decimal)
                    return Serialize((decimal)value);
                else
                    throw new Exception($"Type {value.GetType().Name} not supported");
            }
        }

        internal static MetaFieldData Serialize(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            return new MetaFieldData((byte)bytes.Length, bytes);
        }

        internal static MetaFieldData Serialize(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            return new MetaFieldData((byte)bytes.Length, bytes);
        }

        internal static MetaFieldData Serialize(decimal value)
        {
            byte[] bytes = new byte[16];

            int[] bits = decimal.GetBits(value);
            int lo = bits[0];
            int mid = bits[1];
            int hi = bits[2];
            int flags = bits[3];

            bytes[0] = (byte)lo;
            bytes[1] = (byte)(lo >> 8);
            bytes[2] = (byte)(lo >> 0x10);
            bytes[3] = (byte)(lo >> 0x18);
            bytes[4] = (byte)mid;
            bytes[5] = (byte)(mid >> 8);
            bytes[6] = (byte)(mid >> 0x10);
            bytes[7] = (byte)(mid >> 0x18);
            bytes[8] = (byte)hi;
            bytes[9] = (byte)(hi >> 8);
            bytes[10] = (byte)(hi >> 0x10);
            bytes[11] = (byte)(hi >> 0x18);
            bytes[12] = (byte)flags;
            bytes[13] = (byte)(flags >> 8);
            bytes[14] = (byte)(flags >> 0x10);
            bytes[15] = (byte)(flags >> 0x18);

            return new MetaFieldData((byte)bytes.Length, bytes);
        }

        internal static byte[] Serialize(MetaFieldData[] fields)
        {
            int length = 0;
            foreach (var field in fields)
                length += field.Contents.Length + 1;

            byte[] output = new byte[length];

            int index = 0;
            foreach (var field in fields)
            {
                output[index] = field.Length;
                index++;
                field.Contents.CopyTo(output, index);
                index += field.Contents.Length;
            }

            return output;
        }
    }
}