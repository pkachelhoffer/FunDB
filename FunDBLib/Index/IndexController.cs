using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FunDBLib.Index
{
    public class IndexController
    {
        private static IndexController _Instance { get; set; }
        internal static IndexController Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new IndexController();
                return _Instance;
            }
        }

        private Dictionary<Type, Dictionary<Type, CachedIndex>> IndexDictionary { get; set; }

        private IndexController()
        {
            IndexDictionary = new Dictionary<Type, Dictionary<Type, CachedIndex>>();
        }

        internal void AddIndex<TIndexDefinition, TTableDefinition>(string name, Func<TTableDefinition, TIndexDefinition> funcGenerateIndex)
            where TIndexDefinition : struct
            where TTableDefinition : class, new()
        {
            var tableType = typeof(TTableDefinition);
            if (!IndexDictionary.ContainsKey(tableType))
                IndexDictionary.Add(tableType, new Dictionary<Type, CachedIndex>());

            
            if (!IndexDictionary[tableType].ContainsKey(typeof(TIndexDefinition)))
            {
                var index = new FDIndex<TIndexDefinition, TTableDefinition>(name, funcGenerateIndex);
                IndexDictionary[tableType].Add(typeof(TIndexDefinition), new CachedIndex(new FDIndex<TIndexDefinition, TTableDefinition>(name, funcGenerateIndex), false));
            }

            Debug.WriteLine($"Add index {tableType}, {typeof(TIndexDefinition)}");
        }

        internal IEnumerable<(FDIndex<TTableDefinition> Index, bool Loaded)> GetIndexes<TTableDefinition>()
            where TTableDefinition : class, new()
        {
            if (IndexDictionary.ContainsKey(typeof(TTableDefinition)))
                return IndexDictionary[typeof(TTableDefinition)].Select(s => (s.Value.Index as FDIndex<TTableDefinition>, s.Value.Loaded)).ToList();
            else
                return new List<(FDIndex<TTableDefinition> Index, bool Loaded)>();
        }

        internal (FDIndex<TIndexDefinition, TTableDefinition> Index, bool Loaded) GetIndex<TIndexDefinition, TTableDefinition>()
            where TIndexDefinition : struct
            where TTableDefinition : class, new()
        {
            var tableType = typeof(TTableDefinition);
            var indexType = typeof(TIndexDefinition);

            Debug.WriteLine($"Get index {tableType}, {indexType}");
            if (IndexDictionary.ContainsKey(tableType) && IndexDictionary[tableType].ContainsKey(indexType))
            {
                var cachedIndex = IndexDictionary[tableType][indexType];
                return (cachedIndex.Index as FDIndex<TIndexDefinition, TTableDefinition>, cachedIndex.Loaded);
            }

            throw new Exception($"Index for table {typeof(TTableDefinition)}, index {typeof(TIndexDefinition)} not found.");
        }

        public void SetLoaded<TIndexDefinition, TTableDefinition>()
        {
            var tableType = typeof(TTableDefinition);
            var indexType = typeof(TIndexDefinition);
            if (IndexDictionary.ContainsKey(tableType) && IndexDictionary[tableType].ContainsKey(indexType))
                IndexDictionary[tableType][indexType].Loaded = true;
            else
                throw new Exception($"Index for table {typeof(TTableDefinition)}, index {typeof(TIndexDefinition)} not found.");
        }

        private class CachedIndex
        {
            public FDIndex Index { get; set; }
            public bool Loaded { get; set; }

            public CachedIndex(FDIndex index, bool loaded)
            {
                Index = index;
                Loaded = loaded;
            }
        }
    }
}