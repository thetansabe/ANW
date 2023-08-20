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
    public class PermissionDAO : IRepository<Permission>
    {
        private readonly DataAccess<Permission> _dao = null;
        public PermissionDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<Permission>(settings);
            _dao.Context = "permission";
        }
        public async Task<List<Permission>> Find(Expression<Func<Permission, bool>> filter)
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

        public async Task<Permission> FindByName(string name)
        {
            try
            {
                return await _dao.obj.Find(x=>x.Name.ToLower()==name.Trim().ToLower()).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<List<Permission>> FindByCategory(int category)
        {
            try
            {
                return await _dao.obj.Find(x=>x.Category==category).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<List<Permission>> Find(Expression<Func<Permission, bool>> filter, int limit, int offset)
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

        public async Task<List<Permission>> GetAllAsync()
        {
            try
            {
                return await _dao.obj.Find(_ => true).SortBy(x=>x.Category).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<Permission>> GetAll(int limit, int offset)
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

        public async Task<Permission> GetById(string id)
        {
            var filter = Builders<Permission>.Filter.Eq(x=>x.Id, id);
            try
            {
                return await _dao.obj.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(Permission obj)
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

        public async Task<bool> Update(string id, Permission obj)
        {
            var filter = Builders<Permission>.Filter.Eq(x=>x.Id, id);
            var target = Builders<Permission>.Update
                .Set(s => s.Name, obj.Name)
                .Set(s => s.Id, obj.Id)
                .Set(s => s.Desc, obj.Desc)
                .Set(s => s.Category, obj.Category)
                .Set(s => s.Price, obj.Price);
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
            var filter = Builders<Permission>.Filter.Eq(x=>x.Id, id);
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

        public async Task<long> CountIf(Expression<Func<Permission, bool>> filter)
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
