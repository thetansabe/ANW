using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Amnhac.Interfaces;
using Amnhac.Models;
using System.Linq.Expressions;

namespace Amnhac.Repositories
{
    public class VideoDAO : IRepository<Video>
    {
        private readonly DataAccess<Video> _dao = null;
        public VideoDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<Video>(settings);
            _dao.Context = "video";
        }
        public async Task<List<Video>> Find(Expression<Func<Video, bool>> filter)
        {
            try
            {
                return await _dao.obj.Find(filter).SortByDescending(x => x.CreatedOn).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        
        public async Task<List<Video>> Find(Expression<Func<Video, bool>> filter, int limit, int offset)
        {
            try
            {
                return await _dao.obj.Find(filter).SortByDescending(x=>x.CreatedOn).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Video>> GetAllAsync()
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

        public async Task<List<Video>> GetAll(int limit, int offset)
        {
            try
            {
                return await _dao.obj.Find(_=>true).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task<List<Video>> GetRandom(int limit)
        {
            try
            {
                return await _dao.obj.AsQueryable().Where(x=>x.Approve>0).Sample(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<Video> GetById(string id)
        {
            var filter = Builders<Video>.Filter.Eq("Id", id);
            try
            {
                return await _dao.obj.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(Video obj)
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
        
        public async Task<bool> Update(string id, Video obj)
        {
            var filter = Builders<Video>.Filter.Eq(x => x.Id, id);
            var target = Builders<Video>.Update
                .Set(s => s.Name, obj.Name)
                .Set(s => s.Artists, obj.Artists)
                .Set(s => s.CreatedOn, obj.CreatedOn)
                .Set(s => s.Path, obj.Path)
                .Set(s => s.Extension, obj.Extension)
                .Set(s => s.Thumbnail, obj.Thumbnail)
                .Set(s => s.ThumbnailExtesion, obj.ThumbnailExtesion)
                .Set(s => s.Approve, obj.Approve)
                .Set(s => s.View, obj.View)
                .Set(s => s.UploadBy, obj.UploadBy)
                .Set(s => s.Tags, obj.Tags)
                .CurrentDate(s => s.UpdatedOn);
            var ref_filter= Builders<Song>.Filter.Eq(x=> x.UploadedBy, id.ToString());
            var ref_target = Builders<Song>.Update.Set(s => s.UploadedBy, obj.Id.ToString());
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
            var filter = Builders<Video>.Filter.Eq("Id", id);
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
                long result = await _dao.obj.CountDocumentsAsync(_=>true);
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        
        public async Task<long> CountIf(Expression<Func<Video, bool>> filter)
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
    }
}
