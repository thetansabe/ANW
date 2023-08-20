using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Amnhac.Interfaces;
using Amnhac.Models;
using System.Linq.Expressions;

namespace Amnhac.Repositories
{
    public class ArtistDAO : IRepository<Artist>
    {
        private readonly DataAccess<Artist> _dao = null;
        public ArtistDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<Artist>(settings);
            _dao.Context = "artist";
            _dao.obj.Indexes.CreateOneAsync(new CreateIndexModel<Artist>(
                new IndexKeysDefinitionBuilder<Artist>().Text(x => x.Name)));
        }
        public async Task<List<Artist>> Find(Expression<Func<Artist, bool>> filter)
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

        public async Task<List<Artist>> Find(Expression<Func<Artist, bool>> filter, int limit, int offset)
        {
            try
            {
                return await _dao.obj.Find(filter).SortBy(x=>x.Name).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task<Artist> FindByName(string name)
        {
            try
            {
                return await _dao.obj.Find(c=>c.Name.ToLower()==name.ToLower()).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task<List<Artist>> SmartSearch(string[] keys,int limit, int offset)
        {
            var filter = Builders<Artist>.Filter.Where(x=>x.Name.ToLower().Contains(keys[0]) && x.IsDeleted!=true);
            if (keys.Length>1)
                for (var i=1; i< keys.Length;i++)
                    filter = filter & Builders<Artist>.Filter.Where(x => x.Name.ToLower().Contains(keys[i]));
            try
            {
                return await _dao.obj.Find(filter).SortBy(x => x.Name).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task<List<Artist>> GetAllAsync()
        {
            try
            {
                return await _dao.obj.Find(x=>x.IsDeleted!=true).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Artist>> GetAll(int limit, int offset)
        {
            try
            {
                return await _dao.obj.Find(x => x.IsDeleted != true).SortBy(x=>x.Name).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<Artist> GetById(string id)
        {
            var filter = Builders<Artist>.Filter.Eq(x=>x.Id, id);
            try
            {
                return await _dao.obj.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(Artist obj)
        {
            try
            {
                await _dao.obj.InsertOneAsync(obj);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> Update(string id, Artist obj)
        {
            var filter = Builders<Artist>.Filter.Eq(x=>x.Id, id);
            var target = Builders<Artist>.Update
                .Set(s => s.Name, obj.Name)
                .Set(s => s.AvatarImg, obj.AvatarImg)
                .Set(s => s.Id, obj.Id)
                .Set(s => s.BackgroundImg, obj.BackgroundImg)
                .Set(s => s.DateOfBirth, obj.DateOfBirth)
                .Set(s => s.BelongTo, obj.BelongTo)
                .Set(s => s.Desc, obj.Desc)
                .Set(s => s.Country, obj.Country)
                .Set(s => s.Alphabet, obj.Alphabet)
                .Set(s => s.IsDeleted, obj.IsDeleted)
                .Set(s => s.Deactivated, obj.Deactivated)
                .Set(s => s.Page, obj.Page);
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
        public async Task<bool> Delete(string id)
        {
            var filter = Builders<Artist>.Filter.Eq(x=>x.Id, id);
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

        public async Task<long> CountAll()
        {
            try
            {
                long result = await _dao.obj.CountDocumentsAsync(x => x.IsDeleted != true);
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<long> CountIf(Expression<Func<Artist, bool>> filter)
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
        public async Task<long> CountSmartSearch(string[] keys)
        {
            var filter = Builders<Artist>.Filter.Where(x => x.Name.ToLower().Contains(keys[0]) && x.IsDeleted != true );
            if (keys.Length > 1)
                for (var i = 1; i < keys.Length; i++)
                    filter = filter & Builders<Artist>.Filter.Where(x => x.Name.ToLower().Contains(keys[i]));
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


        public async Task<List<Artist>> Lookup(string keyword, int limit=10)
        {
            try
            {
                var P = Builders<Artist>.Projection.MetaTextScore("TextMatchScore");
                var S = Builders<Artist>.Sort.MetaTextScore("TextMatchScore");
                var list = await _dao.obj.Find(Builders<Artist>.Filter.Text(keyword)
                    )
                    .Project<Artist>(P)
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
