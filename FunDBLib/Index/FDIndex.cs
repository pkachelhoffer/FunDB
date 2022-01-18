using System;
using System.Collections.Generic;
using System.Linq;

namespace FunDBLib.Index
{
    internal abstract class FDIndex<TTableDefinition>
    {
        internal abstract Type IndexDefinitionType { get; }

        public abstract void MaintainRowAdd(TTableDefinition tableDefinition, long address);

        public abstract void MaintainRowAddUpdate(TTableDefinition tableDefinition, long address);

        public abstract void MaintainRowDelete(long address);

        public abstract void EndUpdate();
    }

    internal class FDIndex<TIndexDefinition, TTableDefinition> : FDIndex<TTableDefinition>
        where TIndexDefinition : struct
        where TTableDefinition : class, new()
    {
        private List<IndexItem<TIndexDefinition>> DataList { get; set; }
        private ILookup<TIndexDefinition, long> SeekLookup { get; set; }

        private Dictionary<long, IndexItem<TIndexDefinition>> AddressDictionary { get; set; }

        private Func<TTableDefinition, TIndexDefinition> FuncGenerateIndex { get; set; }

        internal override Type IndexDefinitionType => GetType().GenericTypeArguments[0];

        public FDIndex(Func<TTableDefinition, TIndexDefinition> funcGenerateIndex)
        {
            DataList = new List<IndexItem<TIndexDefinition>>();
            AddressDictionary = new Dictionary<long, IndexItem<TIndexDefinition>>();

            FuncGenerateIndex = funcGenerateIndex;
        }

        public void StartUpdate()
        {
            AddressDictionary.Clear();
        }

        public override void MaintainRowAdd(TTableDefinition tableDefinition, long address)
        {
            var indexRow = FuncGenerateIndex(tableDefinition);
            var indexItem = new IndexItem<TIndexDefinition>(new ComparableObject<TIndexDefinition>(indexRow), address);

            AddressDictionary.Add(address, indexItem);
        }

        public override void MaintainRowAddUpdate(TTableDefinition tableDefinition, long address)
        {
            var indexRow = FuncGenerateIndex(tableDefinition);
            var indexItem = new IndexItem<TIndexDefinition>(new ComparableObject<TIndexDefinition>(indexRow), address);

            if (AddressDictionary.ContainsKey(address))
                AddressDictionary.Remove(address);

            AddressDictionary.Add(address, indexItem);
        }

        public override void MaintainRowDelete(long address)
        {
            if (AddressDictionary.ContainsKey(address))
                AddressDictionary.Remove(address);
        }

        public override void EndUpdate()
        {
            DataList = DataList.OrderBy(s => s.IndexRow.ContainedObject).ToList();
            SeekLookup = AddressDictionary.Select(s => s.Value).ToLookup(s => s.IndexRow.ContainedObject, s => s.Address);
        }

        public long Seek(TIndexDefinition indexRow)
        {
            if (SeekLookup.Contains(indexRow))
                return SeekLookup[indexRow].First();
            else
                return 0;
        }

        private struct IndexItem<T>
        {
            public ComparableObject<T> IndexRow { get; set; }

            public long Address { get; set; }

            public IndexItem(ComparableObject<T> indexRow, long address)
            {
                IndexRow = indexRow;
                Address = address;
            }
        }
    }
}