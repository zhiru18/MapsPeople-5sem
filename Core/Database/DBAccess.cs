﻿using Core.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace Core.Database {
    class DBAccess : IDBAccess {
        MongoClient Client { get; set; }
        IMongoDatabase Database { get; set; }
        IMongoCollection<Script> Collection { get; set; }
        public DBAccess() {
            try {
                Client = new MongoClient("mongodb://localhost:27017");
                Database = Client.GetDatabase("MapsPeople");
                Collection = Database.GetCollection<Script>("Scripts");
            }
            catch (MongoException me) {
                throw new Exception("Something went wrong when trying to connect to the database", me);
            }
        }
        public DBAccess(DBConfig dBConfig) {
            try {
                Client = new MongoClient(dBConfig.ConnectionString);
                Database = Client.GetDatabase(dBConfig.Database);
                Collection = Database.GetCollection<Script>(dBConfig.Collection);
            }
            catch (MongoException me) {
                throw new Exception("Something went wrong when trying to connect to the database", me);
            }
        }
        public void Delete(Script script) {
            var filter = Builders<Script>.Filter.Eq("_id", script.Id);
            Collection.DeleteOneAsync(filter);
        }

        public IEnumerable<Script> GetAll() {
            var scripts = Collection.Find<Script>(f => true).ToListAsync();
            return scripts.Result;
        }

        public Script GetScriptById(string id) {
            Script script = null;
            try {
                if (id != null) {
                    var filter = Builders<Script>.Filter.Eq("_id", id);
                    script = Collection.Find(filter).FirstOrDefault();
                }
            }
            catch (MongoException me) {
                throw new Exception("Something went wrong when trying to get a script", me);
            }
            return script;
        }

        public void Upsert(Script script) {
            try {
                var filter = Builders<Script>.Filter.Eq("_id", script.Id);
                Collection.ReplaceOneAsync(filter, script, new ReplaceOptions { IsUpsert = true });
            }
            catch (MongoException me) {
                throw new Exception("Something went wrong when trying to insert a script", me);
            }
        }
    }
}
