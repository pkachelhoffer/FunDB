using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunDB
{
    public class TestRecord
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }
        public decimal BankBalance { get; set; }

        public TestRecord()
        {

        }

        public TestRecord(string name, string surname, int age, decimal bankBalance)
        {
            Name = name;
            Surname = surname;
            Age = age;
            BankBalance = bankBalance;
        }

        public byte[] SerializeLine()
        {
            var name = BinaryHelper.Serialize(Name);
            var surname = BinaryHelper.Serialize(Surname);
            var age = BinaryHelper.Serialize(Age);
            var bankBalance = BinaryHelper.Serialize(BankBalance);

            return Serialize(new BinaryField[] { name, surname, age, bankBalance });
        }

        private byte[] Serialize(BinaryField[] fields)
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

        public void Deserialise(byte[] content)
        {
            List<BinaryField> fields = new List<BinaryField>();

            int index = 0;
            while (index < content.Length)
            {
                index = ReadField(content, index, out BinaryField field);
                fields.Add(field);
            }

            Name = BinaryHelper.DeserializeString(fields[0].Contents);
            Surname = BinaryHelper.DeserializeString(fields[1].Contents);
            Age = BinaryHelper.DeserializeInt(fields[2].Contents);
            BankBalance = BinaryHelper.DeserializeDecimal(fields[3].Contents);
        }

        private int ReadField(byte[] content, int index, out BinaryField field)
        {
            byte length = content[index];

            int targetIndex = 0;
            byte[] relevantContent = new byte[length];
            for (int x = index + 1; x < index + 1 + length; x++)
            {
                relevantContent[targetIndex] = content[x];
                targetIndex++;
            }

            field = new BinaryField(length, relevantContent);

            return index + length + 1;
        }
    }
}
