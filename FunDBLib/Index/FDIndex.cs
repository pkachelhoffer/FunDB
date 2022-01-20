using System;
using System.Collections.Generic;
using System.Linq;

namespace FunDBLib.Index
{
    internal abstract class FDIndex
    {
        public string Name { get; protected set; }

        public FDIndex(string name)
        {
            Name = name;
        }
    }

    internal abstract class FDIndex<TTableDefinition> : FDIndex
    {
        internal abstract Type IndexDefinitionType { get; }

        public FDIndex(string name) : base(name)
        {

        }

        public abstract void MaintainRowAdd(TTableDefinition tableDefinition, long address);

        public abstract void MaintainRowAddUpdate(TTableDefinition tableDefinition, long address);

        public abstract void MaintainRowDelete(long address);

        public abstract void EndUpdate();

        public abstract void Reset();
    }

    internal class FDIndex<TIndexDefinition, TTableDefinition> : FDIndex<TTableDefinition>
        where TIndexDefinition : struct
        where TTableDefinition : class, new()
    {
        private Dictionary<TIndexDefinition, List<long>> SeekLookup { get; set; }

        private Dictionary<long, IndexItem<TIndexDefinition>> AddressDictionary { get; set; }

        private Func<TTableDefinition, TIndexDefinition> FuncGenerateIndex { get; set; }

        internal override Type IndexDefinitionType => GetType().GenericTypeArguments[0];

        public FDIndex(string name, Func<TTableDefinition, TIndexDefinition> funcGenerateIndex) : base(name)
        {
            AddressDictionary = new Dictionary<long, IndexItem<TIndexDefinition>>();

            FuncGenerateIndex = funcGenerateIndex;
        }

        public void StartUpdate()
        {
            AddressDictionary.Clear();
        }

        public override void Reset()
        {
            AddressDictionary = new Dictionary<long, IndexItem<TIndexDefinition>>();
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
            RefreshSeekLookup();
        }

        private void RefreshSeekLookup()
        {
            SeekLookup = new Dictionary<TIndexDefinition, List<long>>();
            foreach (var entry in AddressDictionary)
            {
                if (!SeekLookup.ContainsKey(entry.Value.IndexRow.ContainedObject))
                    SeekLookup.Add(entry.Value.IndexRow.ContainedObject, new List<long>());

                SeekLookup[entry.Value.IndexRow.ContainedObject].Add(entry.Key);
            }
        }

        public long Seek(TIndexDefinition indexRow)
        {
            if (SeekLookup.ContainsKey(indexRow))
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