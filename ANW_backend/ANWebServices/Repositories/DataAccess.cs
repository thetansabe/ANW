
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using Microsoft.Extensions.Options;

namespace Amnhac.Repositories
{
    public class DataAccess<T>
    {
        private readonly IMongoDatabase _database;
        private string table = "";

        public DataAccess(IOptions<DbSettings> settings)
        {
            try
            {
                var client = new MongoClient(settings.Value.ConnectionString);
                if (client != null)
                    _database = client.GetDatabase(settings.Value.Database);
            }
            catch (Exception ex)
            {
                throw new Exception("Can not access to MongoDb server.", ex);
            }
            
        }
        public string Context
        {
            set { table = value; }
            get { return table; }
        }


        public IMongoCollection<T> obj => _database.GetCollection<T>(table);

    }
}
