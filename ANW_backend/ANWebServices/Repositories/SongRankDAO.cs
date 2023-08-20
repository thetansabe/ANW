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
    public class SongRankDAO : IRepository<SongRanked>
    {
        private readonly DataAccess<SongRanked> _dao = null;
        public SongRankDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<SongRanked>(settings);
            _dao.Context = "songranked";
        }

        public async Task<List<SongRanked>> Find(Expression<Func<SongRanked, bool>> filter)
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

        public async Task<List<SongRanked>> Find(Expression<Func<SongRanked, bool>> filter, int limit, int offset)
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
        public async Task<List<SongRanked>> GetAllAsync()
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

        public async Task<List<SongRanked>> GetAll(int limit, int offset)
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


        public async Task<List<SongRanked>> GetAllInRange(DateTime which)
        {
            try
            {
                return await _dao.obj.Find(x=> x.ValidTo.CompareTo(which)>=0 && x.ValidFrom.CompareTo(which)<=0).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task<SongRanked> GetById(string id)
        {
            try
            {
                return await _dao.obj.Find(x => x.SongId == id).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(SongRanked obj)
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

        public async Task<bool> Update(string id, SongRanked obj)
        {
            var filter = Builders<SongRanked>.Filter.Eq("SongId", id);
            UpdateDefinition<SongRanked> target;
            if (obj != null)
                target= Builders<SongRanked>.Update
                .Set(s => s.Priority, obj.Priority)
                .Set(s => s.ValidFrom, obj.ValidFrom)
                .Set(s => s.ValidTo, obj.ValidTo)
                .CurrentDate(s => s.LastUpdate);
            else
                target= Builders<SongRanked>.Update.CurrentDate(s => s.LastUpdate);
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
            var filter = Builders<SongRanked>.Filter.Eq(x=>x.SongId, id);
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

        public async Task<long> CountIf(Expression<Func<SongRanked, bool>> filter)
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
