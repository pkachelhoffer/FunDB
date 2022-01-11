using System;
using System.IO;

namespace FunDBLib
{
    public class FDDataReader<TTableDefinition> : IDisposable
        where TTableDefinition : class, new()
    {
        private FDTable<TTableDefinition> Table { get; set; }

        private FileStream FileStream { get; set; }

        internal FDDataReader(FDTable<TTableDefinition> table)
        {
            Table = table;

            FileStream = new FileStream(Table.DataPath, FileMode.Open);
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

        public TTableDefinition Seek<TIndexDefinition>(TIndexDefinition indexRow)
            where TIndexDefinition : class, new()
        {
            var index = Table.GetIndex<TIndexDefinition>();

            var address = index.Seek(indexRow, out bool found);

            if (found)
            {
                FileStream.Position = address;
                return DataRecordParser.ReadRecord<TTableDefinition>(FileStream, Table.TableMetaData).Row;
            }
            else
                return null;
        }

        public void Dispose()
        {
            FileStream.Dispose();
        }
    }
}