using JsonFlatFileDataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceWebApi.Tests.Helpers
{
    /// <summary>
    /// Generic mock implementation of IDocumentCollection for testing
    /// </summary>
    internal class MockDocumentCollection<T> : IDocumentCollection<T>
    {
        private readonly List<T> _items;

        public MockDocumentCollection(List<T> items)
        {
            _items = items;
        }

        public int Count => _items.Count;

        public IEnumerable<T> AsQueryable() => _items.AsQueryable();

        public IEnumerable<T> Find(Predicate<T> query) => _items.Where(i => query(i));

        public IEnumerable<T> Find(string text, bool caseSensitive = false) => _items;

        public dynamic GetNextIdValue()
        {
            // For int-keyed collections, return max + 1
            if (typeof(T).GetProperty("Id")?.PropertyType == typeof(int))
            {
                var ids = _items
                    .Select(i => (int)(typeof(T).GetProperty("Id")?.GetValue(i) ?? 0))
                    .ToList();
                return ids.Count > 0 ? ids.Max() + 1 : 1;
            }
            return _items.Count + 1;
        }

        public bool InsertOne(T item) { _items.Add(item); return true; }
        public Task<bool> InsertOneAsync(T item) { _items.Add(item); return Task.FromResult(true); }
        public bool InsertMany(IEnumerable<T> items) { _items.AddRange(items); return true; }
        public Task<bool> InsertManyAsync(IEnumerable<T> items) { _items.AddRange(items); return Task.FromResult(true); }

        public bool ReplaceOne(Predicate<T> filter, T item, bool upsert = false) => true;
        public bool ReplaceOne(object id, T item, bool upsert = false) => true;
        public Task<bool> ReplaceOneAsync(Predicate<T> filter, T item, bool upsert = false) => Task.FromResult(true);
        public Task<bool> ReplaceOneAsync(object id, T item, bool upsert = false) => Task.FromResult(true);
        public bool ReplaceMany(Predicate<T> filter, T item) => true;
        public Task<bool> ReplaceManyAsync(Predicate<T> filter, T item) => Task.FromResult(true);

        public bool UpdateOne(Predicate<T> filter, dynamic item) => true;
        public bool UpdateOne(object id, dynamic item) => true;
        public Task<bool> UpdateOneAsync(Predicate<T> filter, dynamic item) => Task.FromResult(true);
        public Task<bool> UpdateOneAsync(object id, dynamic item) => Task.FromResult(true);
        public bool UpdateMany(Predicate<T> filter, dynamic item) => true;
        public Task<bool> UpdateManyAsync(Predicate<T> filter, dynamic item) => Task.FromResult(true);

        public bool DeleteOne(Predicate<T> filter) => true;
        public bool DeleteOne(object id) => true;
        public Task<bool> DeleteOneAsync(Predicate<T> filter) => Task.FromResult(true);
        public Task<bool> DeleteOneAsync(object id) => Task.FromResult(true);
        public bool DeleteMany(Predicate<T> filter) => true;
        public Task<bool> DeleteManyAsync(Predicate<T> filter) => Task.FromResult(true);
    }
}
