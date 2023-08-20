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
    public class SysConfigDAO : IRepository<SystemConfig>
    {
        private readonly DataAccess<SystemConfig> _dao = null;
        public SysConfigDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<SystemConfig>(settings);
            _dao.Context = "cfg";
        }

        public async Task<List<SystemConfig>> Find(Expression<Func<SystemConfig, bool>> filter)
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

        public async Task<List<SystemConfig>> Find(Expression<Func<SystemConfig, bool>> filter, int limit, int offset)
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
        public async Task<List<SystemConfig>> GetAllAsync()
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

        public async Task<List<SystemConfig>> GetAll(int limit, int offset)
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
        

        public async Task<SystemConfig> GetById(string id)
        {
            try
            {
                return await _dao.obj.Find(_=>true).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(SystemConfig obj)
        {
            try
            {
                if (CountAll().Result > 0)
                {
                    return await Update(null, obj);
                }
                await _dao.obj.InsertOneAsync(obj);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> Update(string id, SystemConfig obj)
        {
            if (CountAll().Result > 0)
            {
                var real = await GetById(null);
                id = real.Id;
            }
            var filter = Builders<SystemConfig>.Filter.Eq("Id", id);
            var target = Builders<SystemConfig>.Update
                .Set(s => s.CategoryToChart, obj.CategoryToChart);
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
            var filter = Builders<SystemConfig>.Filter.Eq(x=>x.Id, id);
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

        public async Task<long> CountIf(Expression<Func<SystemConfig, bool>> filter)
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
