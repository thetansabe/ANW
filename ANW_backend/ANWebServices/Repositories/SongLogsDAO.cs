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
    public class SongLogsDAO : IRepository<SongLogs>
    {
        private readonly DataAccess<SongLogs> _dao = null;
        public SongLogsDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<SongLogs>(settings);
            _dao.Context = "songlogs";
        }

        public async Task<List<SongLogs>> Find(Expression<Func<SongLogs, bool>> filter)
        {
            try
            {
                return await _dao.obj.Find(filter).SortByDescending(x=>x.CreatedOn).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<SongLogs>> Find(Expression<Func<SongLogs, bool>> filter, int limit, int offset)
        {
            try
            {
                return await _dao.obj.Find(filter).SortByDescending(x => x.CreatedOn).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<List<SongLogs>> GetAllAsync()
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

        public async Task<List<SongLogs>> GetAll(int limit, int offset)
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

        public async Task<SongLogs> GetFromDay(string id,DateTime which)
        {
            try
            {
                return await _dao.obj.Find(
                    x=>x.SongId==id &&
                    x.CreatedOn.Day==which.Day && 
                    x.CreatedOn.Month==which.Month &&
                    x.CreatedOn.Year==which.Year).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<SongLogs> GetById(string id)
        {
            try
            {
                return await _dao.obj.Find(x => x.SongId == id).SortByDescending(x => x.CreatedOn).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(SongLogs obj)
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

        public async Task<bool> Update(string id, SongLogs obj)
        {
            var filter = Builders<SongLogs>.Filter.Eq("SongId", id);
            var target = Builders<SongLogs>.Update
                .Set(s => s.ViewCount, obj.ViewCount)
                .Set(s => s.SongCategory, obj.SongCategory).CurrentDate(s => s.CreatedOn);
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
            var filter = Builders<SongLogs>.Filter.Eq(x=>x.SongId, id);
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

        public async Task<bool> Clear(DateTime which)
        {
            var filter = Builders<SongLogs>.Filter.Lt(s => s.CreatedOn, which);
            try
            {
                DeleteResult result = await _dao.obj.DeleteManyAsync(filter);
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

        public async Task<long> CountIf(Expression<Func<SongLogs, bool>> filter)
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

        public IMongoCollection<SongLogs> Context
        {
            get { return _dao.obj; }
        }
    }
}
