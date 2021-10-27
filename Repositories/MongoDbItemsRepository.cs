using System;
using System.Collections.Generic;
using Catalog.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
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
        public void CreateItem(Item item)
        {
            _itemsCollection.InsertOne(item);
        }

        public void DeleteItem(Guid id)
        {
            var filter=filterBuilder.Eq(item=>item.Id,id);
            _itemsCollection.DeleteOne(filter);
        }

        public Item GetItem(Guid id)
        {
            var filter=filterBuilder.Eq(item=>item.Id,id);
            return _itemsCollection.Find(filter).SingleOrDefault();
        }

        public IEnumerable<Item> GetItems()
        {
            return _itemsCollection.Find(new BsonDocument()).ToList();
        }

        public void UpdateItem(Item item)
        {
            var filter=filterBuilder.Eq(existingItem=>existingItem.Id,item.Id);
            _itemsCollection.ReplaceOne(filter,item);

        }
    }
}