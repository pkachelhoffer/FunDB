using System;
using System.IO;
using FunDBLib.Index;

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
            where TIndexDefinition : struct
        {
            var cachedIndex = IndexController.Instance.GetIndex<TIndexDefinition, TTableDefinition>();
            if (!cachedIndex.Loaded)
            {
                RefreshIndex(cachedIndex.Index);
                IndexController.Instance.SetLoaded<TIndexDefinition, TTableDefinition>();
            }

            var index = cachedIndex.Index;

            var address = index.Seek(indexRow);

            if (address > 0)
            {
                FileStream.Position = address;
                var record = DataRecordParser.ReadRecord<TTableDefinition>(FileStream, Table.TableMetaData).Row;
                Table.RecordReadTracker.Add(record, address);
                return record;
            }
            else
                return null;
        }

        public void Dispose()
        {
            FileStream.Dispose();
        }

        private void RefreshIndex(FDIndex<TTableDefinition> index)
        {
            FileStream.Position = Table.HeaderData.FirstRecordPosition;

            long address = FileStream.Position;

            while (ReadLine(out TTableDefinition row))
            {
                index.MaintainRowAdd(row, address);
                address = FileStream.Position;
            }

            index.EndUpdate();
        }
    }
}