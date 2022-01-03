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
            byte[] stringLengthBytes = content.FDCopyArray(0, 4);
            int stringLength = DeserializeInt(stringLengthBytes);

            byte[] stringBytes = content.FDCopyArray(4, stringLength);
            return Encoding.UTF8.GetString(stringBytes);
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

        internal static byte[] Serialize(object? valueNullable)
        {
            if (valueNullable == null)
                return new byte[0];
            else
            {
                byte[] convertedBytes;

                var value = (object)valueNullable;

                if (value is int || value.GetType().IsEnum)
                    convertedBytes = BitConverter.GetBytes((int)value);
                else if (value is string)
                    convertedBytes = SerializeString((string)value);
                else if (value is decimal)
                    convertedBytes = Serialize((decimal)value);
                else if (value is byte)
                    convertedBytes = new byte[1] { (byte)value };
                else
                    throw new Exception($"Type {value.GetType().Name} not supported");

                if (convertedBytes == null)
                    throw new Exception("Converting field to bytes failed");

                return convertedBytes;
            }
        }

        private static byte[] SerializeString(string stringValue)
        {
            byte[] stringBytes = Encoding.UTF8.GetBytes(stringValue);
            byte[] lengthBytes = BitConverter.GetBytes(stringBytes.Length);
            byte[] convertedBytes = new byte[stringBytes.Length + lengthBytes.Length];

            for (int x = 0; x < lengthBytes.Length; x++)
                convertedBytes[x] = lengthBytes[x];

            for (int x = 0; x < stringBytes.Length; x++)
                convertedBytes[x + lengthBytes.Length] = stringBytes[x];

            return convertedBytes;
        }

        internal static byte[] Serialize(decimal value)
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

            return bytes;
        }

        internal static byte[] FDCopyArray(this byte[] fromArray, int startIndex, int length)
        {
            byte[] outArray = new byte[length];
            int index = 0;
            for (int x = startIndex; x < startIndex + length; x++)
            {
                outArray[index] = fromArray[x];
                index++;
            }

            return outArray;
        }

        private static byte[] TrimBytes(byte[] inputBytes, int length)
        {
            byte[] newBytes = new byte[length];

            for (int x = 0; x < length; x++)
                newBytes[x] = inputBytes[x];

            return newBytes;
        }
    }
}