using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Api;
using System.Linq.Expressions;

namespace Amnhac.Repositories
{
    public class SongDAO : IRepository<Song>
    {
        private readonly DataAccess<Song> _dao = null;
        public SongDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<Song>(settings);
            _dao.Context = "song";
            _dao.obj.Indexes.CreateOneAsync(new CreateIndexModel<Song>(
                new IndexKeysDefinitionBuilder<Song>().Text(x=>x.Tags)));
        }

        public async Task<List<Song>> Find(string[] key, string[] val)
        {
            var filter = Builders<Song>.Filter.Eq(key[0], val[0]);
            for (int i=1; i<key.Length;i++)
                filter=filter & Builders<Song>.Filter.Eq(key[i], val[i]);
            try
            {
                return await _dao.obj.Find(filter).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Song>> Find(Expression<Func<Song, bool>> filter)
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


        public async Task<List<Song>> Find(Expression<Func<Song, bool>>  filter, int limit, int offset)
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
        public async Task<List<Song>> FindSorted(string[] key, string[] val, int limit, int offset)
        {
            var filter = Builders<Song>.Filter.Eq(key[0], val[0]);
            for (int i = 1; i < key.Length; i++)
                filter = filter & Builders<Song>.Filter.Eq(key[i], val[i]);
            try
            {
                return await _dao.obj.Find(filter).SortByDescending(x => x.CreatedOn).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Song>> SmartFinder(string keywords, int minLevel, int approveLevel, int limit, int offset, string userId=null)
        {
            try
            {
                string keyword = CharUtil.ConvertToNonUnicode(keywords).ToLower();
                if (userId!=null)
                    return await _dao.obj.Find(x => x.Tags.Contains(keyword) && x.Approved>=minLevel && x.Approved<=approveLevel && x.UploadedBy==userId)
                    .SortByDescending(x => x.CreatedOn).Skip(offset).Limit(limit).ToListAsync();
                else
                    return await _dao.obj.Find(x => x.Tags.Contains(keyword) && x.Approved >= minLevel && x.Approved <= approveLevel)
                    .SortByDescending(x => x.CreatedOn).Skip(offset).Limit(limit).ToListAsync();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task<List<Song>> GetAllAsync()
        {
            try
            {
                return await _dao.obj.Find(_ => true).ToListAsync();
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Song>> GetAll(int limit, int offset)
        {
            try
            {
                return await _dao.obj.Find(_=>true).SortByDescending(x=>x.CreatedOn).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        public async Task<List<Song>> GetAllWithFilter(int minLevel, int approveLevel,int limit, int offset, string userId=null)
        {
            try
            {
                if (userId!=null)
                    return await _dao.obj.Find(s => s.Approved <= approveLevel && s.Approved>=minLevel && s.UploadedBy==userId).SortByDescending(x => x.CreatedOn).Skip(offset).Limit(limit).ToListAsync();
                else
                return await _dao.obj.Find(s=>s.Approved<=approveLevel && s.Approved >= minLevel).SortByDescending(x => x.CreatedOn).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        

        public async Task<List<Song>> GetAllByView(int limit, int offset)
        {
            try
            {
                var query = Builders<Song>.Filter;
                var filter =  query.And(query.Gt("Approved", 0),
                    query.Eq("SelfPerformance", 0));
                var filter2 = query.And(query.Gt("Approved", 0),
                    query.Eq("SelfPerformance", BsonNull.Value));
                return await _dao.obj.Find(query.Or(filter,filter2)).SortByDescending(x=>x.View).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<Song> GetById(string id)
        {
            try
            {
                return await _dao.obj.Find(x=>x.Id==id).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(Song obj)
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

        public async Task<bool> Update(string id, Song obj)
        {
            var filter = Builders<Song>.Filter.Eq(x=>x.Id, id);
            if (obj.View > 99999999999) obj.View = 99999999999;
            var target = Builders<Song>.Update
                .Set(s => s.Name, obj.Name)
                .Set(s => s.Approved, obj.Approved)
                .Set(s => s.Id, obj.Id)
                .Set(s => s.Lyrics, obj.Lyrics)
                .Set(s => s.Artists, obj.Artists)
                .Set(s => s.Paths, obj.Paths)
                .Set(s => s.View, obj.View)
                .Set(s => s.RefVideo, obj.RefVideo)
                .Set(s => s.UploadedBy, obj.UploadedBy)
                .Set(s => s.Price, obj.Price)
                .Set(s => s.SubType, obj.SubType)
                .Set(s => s.HitLove, obj.HitLove)
                .Set(s => s.HitDownload, obj.HitDownload)
                .Set(s => s.Tags, obj.Tags)
                .Set(s => s.SelfPerformance, obj.SelfPerformance)
                .Set(s => s.IsDeleted, obj.IsDeleted)
                .CurrentDate(s => s.UpdatedOn);
            try
            {
                UpdateResult result = await _dao.obj.UpdateOneAsync(filter,target);
                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> Delete(string id)
        {
            var filter = Builders<Song>.Filter.Eq(x=>x.Id,id);
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

        public async Task<long> CountByUser(string id)
        {
            try
            {
                long result = await _dao.obj.Find(x=>x.UploadedBy==id).CountDocumentsAsync();
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public async Task<long> CountWithFilter(int minLevel, int approveLevel, string userId=null)
        {
            try
            {
                long result = 0;
                if (userId != null) result =
                   await _dao.obj.Find(x => x.UploadedBy == userId && x.Approved >= minLevel && x.Approved <= approveLevel).CountDocumentsAsync();
                else result= await _dao.obj.Find(x => x.Approved <= approveLevel && x.Approved >= minLevel).CountDocumentsAsync();
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }


        public async Task<long> CountAll()
        {
            try
            {
                long result = await _dao.obj.CountDocumentsAsync(_=>true);
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public async Task<long> CountByType(string id)
        {
            try
            {
                long result = await _dao.obj.CountDocumentsAsync(s =>s.SubType.Equals(id));
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }


        public async Task<long> CountBySmartFind(string keywords, int minLevel,int approveLevel, string userId=null)
        {
            try
            {
                string keyword = CharUtil.ConvertToNonUnicode(keywords).ToLower();
                long result = 0;
                if (userId!=null)
                    result=await _dao.obj.Find(x => x.Tags.Contains(keyword) && x.Approved<=approveLevel && x.Approved >= minLevel && x.UploadedBy==userId.ToString())
                        .CountDocumentsAsync();
                else
                    result = await _dao.obj.Find(x => x.Tags.Contains(keyword) && x.Approved <= approveLevel && x.Approved >= minLevel).CountDocumentsAsync();
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<long> CountIf(Expression<Func<Song, bool>> filter)
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

        public async Task<List<Song>> Lookup(string keyword, int limit=10)
        {
            try
            {
                var P = Builders<Song>.Projection.MetaTextScore("TextMatchScore");
                var S = Builders<Song>.Sort.MetaTextScore("TextMatchScore").Descending(x=>x.View);
                var list = await _dao.obj.Find((Builders<Song>.Filter.Text(keyword)
                    )
                    & !Builders<Song>.Filter.Eq(x=>x.IsDeleted,true)
                    & Builders<Song>.Filter.Gt(x=>x.Approved,0)
                    )
                    .Project<Song>(P)
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
