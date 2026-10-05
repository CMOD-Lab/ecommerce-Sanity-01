using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Repositories;
using EcommerceWebApi.Services;
using EcommerceWebApi.Utilities;
using JsonFlatFileDataStore;
using Moq;

namespace EcommerceWebApi.Services.Tests
{
    // Mock IDocumentCollection for testing - implements the actual JsonFlatFileDataStore interface
    public class MockDocumentCollection<T> : IDocumentCollection<T>
    {
        private readonly List<T> _items;

        public MockDocumentCollection(List<T> items)
        {
            _items = items ?? new List<T>();
        }

        public int Count => _items.Count;

        public IEnumerable<T> AsQueryable() => _items;

        public IEnumerable<T> Find(Predicate<T> query) => _items.Where(x => query(x));

        public IEnumerable<T> Find(string text, bool caseSensitive = false) => _items;

        public dynamic GetNextIdValue() => _items.Count + 1;

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
