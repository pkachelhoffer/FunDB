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
            FileStream.Position = Table.HeaderData.FirstRecordPosition;
        }

        internal FDDataReader(FDTable table, FileStream fileStream)
        {
            Table = table;
            FileStream = fileStream;
            FileStream.Position = Table.HeaderData.FirstRecordPosition;
        }

        public bool ReadLine(out TTableDefinition row)
        {
            if (FileStream.Position == 0)
            {
                row = null;
                return false;
            }
            else
            {
                long address = FileStream.Position;

                var record = DataRecordParser.ReadRecord<TTableDefinition>(FileStream, Table.TableMetaData);
                FileStream.Position = record.NextAddress;
                row = record.Row;

                Table.RecordReadTracker.Add(record.Row, address);

                return true;
            }
        }

        public void Dispose()
        {
            FileStream.Dispose();
        }
    }
}