using Amnhac.Interfaces;
using Amnhac.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Amnhac.Repositories
{
    public class PlaylistDAO : IRepository<Playlist>
    {
        private readonly DataAccess<Playlist> _dao = null;
        public PlaylistDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<Playlist>(settings);
            _dao.Context = "playlist";
            _dao.obj.Indexes.CreateOneAsync(new CreateIndexModel<Playlist>(
                new IndexKeysDefinitionBuilder<Playlist>().Text(x => x.Tags)));
        }
        public async Task<long> CountAll()
        {
            try
            {
                long result = await _dao.obj.CountDocumentsAsync(_ => true);
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public async Task<long> CountAll(string userId)
        {
            try
            {
                long result = await _dao.obj.Find(s=>s.UserId==userId).CountDocumentsAsync();
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }


        public async Task<bool> Delete(string id)
        {
            var filter = Builders<Playlist>.Filter.Eq(x => x.Id, id);
            try
            {
                DeleteResult result = await _dao.obj.DeleteOneAsync(filter);
                return result.IsAcknowledged && result.DeletedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<Playlist>> Find(Expression<Func<Playlist, bool>> filter)
        {
            try
            {
                return await _dao.obj.Find(filter).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Playlist>> Find(Expression<Func<Playlist, bool>> filter, int limit, int offset)
        {
            try
            {
                return await _dao.obj.Find(filter).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Playlist>> GetAllAsync()
        {
            try
            {
                return await _dao.obj.Find(_ => true).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Playlist>> GetAll(int limit, int offset)
        {
            try
            {
                return await _dao.obj.Find(_ => true).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<Playlist> GetById(string id)
        {
            var filter = Builders<Playlist>.Filter.Eq(x => x.Id, id);
            try
            {
                return await _dao.obj.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(Playlist obj)
        {
            try
            {
                await _dao.obj.InsertOneAsync(obj);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> Update(string id, Playlist obj)
        {
            var filter = Builders<Playlist>.Filter.Eq(x => x.Id, id);
            var target = Builders<Playlist>.Update
                .Set(s => s.Name, obj.Name)
                .Set(s => s.Id, obj.Id)
                .Set(s => s.Collection, obj.Collection)
                .Set(s => s.View, obj.View)
                .Set(s => s.Public, obj.Public)
                .Set(s => s.Tags, obj.Tags);
            try
            {
                UpdateResult result = await _dao.obj.UpdateOneAsync(filter, target);
                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<List<Playlist>> FindByUserId(string userId, int limit, int offset)
        {
            try
            {
                return await _dao.obj.Find(x => x.UserId.Equals(userId)).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Playlist>> Filter(string keyword, int limit)
        {
            try
            {
                return await _dao.obj.Find(x=>x.Tags.Contains(keyword)).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Playlist>> Filter(string keyword, int limit, string userId)
        {
            try
            {
                return await _dao.obj.Find(x => x.Tags.Contains(keyword) && x.UserId.Equals(userId)).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Playlist>> FilterEach(string[] keyword, int limit)
        {
            if (keyword.Length <= 0) return null;
            var filter = Builders<Playlist>.Filter.Where(x => x.Tags.Contains(keyword[0]));
            for (var i=1; i<keyword.Length;i++)
                filter=filter & Builders<Playlist>.Filter.Where(x => x.Tags.Contains(keyword[i]));
            try
            {
                return await _dao.obj.Find(filter).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Playlist>> FilterEach(string[] keyword, int limit, string userId)
        {
            if (keyword.Length <= 0) return null;
            var filter = Builders<Playlist>.Filter.Where(x => x.Tags.Contains(keyword[0]));
            for (var i = 1; i < keyword.Length; i++)
                filter = filter & Builders<Playlist>.Filter.Where(x => x.Tags.Contains(keyword[i]));

            filter = filter & Builders<Playlist>.Filter.Where(x => x.UserId.Equals(userId));
            try
            {
                return await _dao.obj.Find(filter).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<long> CountIf(Expression<Func<Playlist, bool>> filter)
        {
            try
            {
                long result = await _dao.obj.CountDocumentsAsync(filter);
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<List<Playlist>> Lookup(string keyword, int limit=10)
        {
            try
            {
                var P = Builders<Playlist>.Projection.MetaTextScore("TextMatchScore");
                var S = Builders<Playlist>.Sort.MetaTextScore("TextMatchScore");
                var list = await _dao.obj.Find((Builders<Playlist>.Filter.Text(keyword)
                    )
                    & Builders<Playlist>.Filter.Gt(x=>x.Public,0)
                    )
                    .Project<Playlist>(P)
                    .Sort(S)
                    .Limit(limit).ToListAsync();
                return list;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
    }
}
