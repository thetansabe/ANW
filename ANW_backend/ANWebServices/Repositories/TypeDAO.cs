using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Amnhac.Interfaces;
using Amnhac.Models;
using System.Linq.Expressions;

namespace Amnhac.Repositories
{
    public class TypeDAO : IRepository<SongType>
    {
        private readonly DataAccess<SongType> _dao = null;
        public TypeDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<SongType>(settings);
            _dao.Context = "songtype";
        }
        public async Task<List<SongType>> Find(Expression<Func<SongType, bool>> filter)
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

        public async Task<List<SongType>> Find(Expression<Func<SongType, bool>> filter, int limit, int offset)
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

        public async Task<List<SongType>> GetAllAsync()
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

        public async Task<List<SongType>> GetAll(int limit, int offset)
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

        public async Task<SongType> GetById(string id)
        {
            var filter = Builders<SongType>.Filter.Eq(x=>x.Id, id.ToString());
            try
            {
                return await _dao.obj.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<SongType> GetBySubId(string id)
        {
            try
            {
                return await _dao.obj.Find(x=>x.SubType.Any(y=>y.Id==id)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public SongSubType GetSubType(SongType parent,string id)
        {
            try
            {
                foreach (SongSubType result in parent.SubType)
                {
                    if (result.Id == id)
                        return result;
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(SongType obj)
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

        public async Task<bool> Update(string id, SongType obj)
        {
            var filter = Builders<SongType>.Filter.Eq(x=>x.Id, id);
            var target = Builders<SongType>.Update
                .Set(s => s.Id, obj.Id)
                .Set(s => s.Text, obj.Text)
                .Set(s=> s.SubType,obj.SubType);
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
            var filter = Builders<SongType>.Filter.Eq(x=>x.Id, id);
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

        public async Task<long> CountIf(Expression<Func<SongType, bool>> filter)
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
