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
    public class AlbumDAO : IRepository<Album>
    {
        private readonly DataAccess<Album> _dao = null;
        public AlbumDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<Album>(settings);
            _dao.Context = "album";
            _dao.obj.Indexes.CreateOneAsync(new CreateIndexModel<Album>(
                new IndexKeysDefinitionBuilder<Album>().Text(x => x.Tags)));
        }
        public async Task<List<Album>> Find(Expression<Func<Album, bool>> filter)
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

        public async Task<List<Album>> Find(Expression<Func<Album, bool>> filter, int limit, int offset)
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


        public async Task<List<Album>> FindByName(string name, int limit)
        {
            try
            {
                string lowerName = name.ToLower();
                return await _dao.obj.Find(c => c.IsDeleted!=true && c.Name.ToLower().Contains(lowerName)).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Album>> SmartFindByName(string name, int limit)
        {
            string lowerName = name.ToLower().Trim();
            string[] word = lowerName.Split(" ");

            var filter = Builders<Album>.Filter.Where(x => x.IsDeleted != true && x.Tags.ToLower().Contains(word[0]));
            for (int i = 1; i < word.Length; i++)
                filter = filter & Builders<Album>.Filter.Where(x => x.Tags.ToLower().Contains(word[i]));
            try
            {
                return await _dao.obj.Find(filter).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public async Task<List<Album>> GetAllAsync()
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

        public async Task<List<Album>> GetAll(int limit, int offset)
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


        public async Task<List<Album>> GetAllByUser(string id, int limit, int offset)
        {
            try
            {
                return await _dao.obj.Find(x=>x.CreatedBy.Equals(id)).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task<Album> GetById(string id)
        {
            var filter = Builders<Album>.Filter.Eq(x=>x.Id, id);
            try
            {
                return await _dao.obj.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(Album obj)
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

        public async Task<bool> Update(string id, Album obj)
        {
            var filter = Builders<Album>.Filter.Eq(x=>x.Id, id);
            var target = Builders<Album>.Update
                .Set(s => s.Name, obj.Name)
                .Set(s => s.AvatarImg, obj.AvatarImg)
                .Set(s => s.Id, obj.Id)
                .Set(s => s.BackgroundImg, obj.BackgroundImg)
                .Set(s => s.Approved, obj.Approved)
                .Set(s => s.Artists, obj.Artists)
                .Set(s => s.Collection, obj.Collection)
                .Set(s => s.Desc, obj.Desc)
                .Set(s => s.Tags, obj.Tags)
                .Set(s => s.View, obj.View)
                .Set(s => s.IsDeleted, obj.IsDeleted)
                .CurrentDate(s => s.UpdatedOn);
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
            var filter = Builders<Album>.Filter.Eq(x=>x.Id, id);
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
                long result = await _dao.obj.CountDocumentsAsync(_ => true);
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public async Task<long> CountAllByUser(string id)
        {
            try
            {
                long result = await _dao.obj.CountDocumentsAsync(s => s.CreatedBy.Equals(id));
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<long> CountIf(Expression<Func<Album, bool>> filter)
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


        public async Task<List<Album>> Lookup(string keyword, int limit=10)
        {
            try
            {
                var P = Builders<Album>.Projection.MetaTextScore("TextMatchScore");
                var S = Builders<Album>.Sort.MetaTextScore("TextMatchScore");
                var list = await _dao.obj.Find(Builders<Album>.Filter.Text(keyword)
                    )
                    .Project<Album>(P)
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
