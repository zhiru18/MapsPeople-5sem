﻿using API.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace API.Database {
    public class DBAccess : IDBAccess {
        MongoClient Client { get; set; }
        IMongoDatabase Database { get; set; }
        IMongoCollection<Script> Collection { get; set; }
        public DBConfig Config { get; set; }
        public DBAccess(DBConfig dBConfig) {
            try {
                Config = dBConfig;
                Client = new MongoClient(Config.ConnectionString);
                Database = Client.GetDatabase(Config.Database);
                Collection = Database.GetCollection<Script>(Config.Collection);
            } catch (MongoException me) {
                throw new Exception("Something went wrong when trying to connect to the database", me);
            }
        }
        public void Delete(string id) {
            var filter = Builders<Script>.Filter.Eq("_id", id);
            Collection.DeleteOne(filter);
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
            } catch (MongoException me) {
                throw new Exception("Something went wrong when trying to get a script", me);
            }
            return script;
        }

        public void Upsert(Script script) {
            try {
                var filter = Builders<Script>.Filter.Eq("_id", script.Id);
                Collection.ReplaceOne(filter, script, new ReplaceOptions { IsUpsert = true });
            } catch (MongoException me) {
                throw new Exception("Something went wrong when trying to insert a script", me);
            }
        }

        public IEnumerable<Script> GetByLanguage(string language) {
            var filter = Builders<Script>.Filter.Eq("Language", language);
            var scripts = Collection.Find<Script>(filter).ToListAsync();
            return scripts.Result;
        }
    }
}
