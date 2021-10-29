using System;
using System.Collections.Generic;
using Catalog.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace Catalog.Repositories{


    public class MongoDbItemsRepository : IItemsRepository
    {
        private readonly IMongoCollection<Item> _itemsCollection;
        private const string _databaseName="catalog";
        private const string _collectionname="items";
        private readonly FilterDefinitionBuilder<Item> filterBuilder =Builders<Item>.Filter;
        // public MongoDbItemsRepository(IMongoClient mongoClient)
        // {
        //     IMongoDatabase database=mongoClient.GetDatabase(_databaseName);
        //     _itemsCollection=database.GetCollection<Item>(_collectionname);

        // }
        public MongoDbItemsRepository(IMongoClient mongoClient)
        {
            IMongoDatabase database=mongoClient.GetDatabase(_databaseName);
            _itemsCollection=database.GetCollection<Item>(_collectionname);
        }
        public async Task CreateItemAsync(Item item)
        {
            await _itemsCollection.InsertOneAsync(item);
        }

        public async Task DeleteItemAsync(Guid id)
        {
            var filter=filterBuilder.Eq(item=>item.Id,id);
            await _itemsCollection.DeleteOneAsync(filter);
        }

        public async Task<Item> GetItemAsync(Guid id)
        {
            var filter=filterBuilder.Eq(item=>item.Id,id);
            return await _itemsCollection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<Item>> GetItemsAsync()
        {
            return await _itemsCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task UpdateItemAsync(Item item)
        {
            var filter=filterBuilder.Eq(existingItem=>existingItem.Id,item.Id);
            await _itemsCollection.ReplaceOneAsync(filter,item);

        }
    }
}