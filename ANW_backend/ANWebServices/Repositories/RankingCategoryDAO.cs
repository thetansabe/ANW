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
    public class RankingCategoryDAO : IRepository<RankingCategory>
    {
        private readonly DataAccess<RankingCategory> _dao = null;
        public RankingCategoryDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<RankingCategory>(settings);
            _dao.Context = "ranking_category";
        }

        public async Task<List<RankingCategory>> Find(Expression<Func<RankingCategory, bool>> filter)
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

        public async Task<List<RankingCategory>> Find(Expression<Func<RankingCategory, bool>> filter, int limit, int offset)
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
        public async Task<List<RankingCategory>> GetAllAsync()
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

        public async Task<List<RankingCategory>> GetAll(int limit, int offset)
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

        

        public async Task<RankingCategory> GetById(string id)
        {
            try
            {
                return await _dao.obj.Find(x => x.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(RankingCategory obj)
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

        public async Task<bool> Update(string id, RankingCategory obj)
        {
            var filter = Builders<RankingCategory>.Filter.Eq("SongId", id);
            UpdateDefinition<RankingCategory> target;
                target = Builders<RankingCategory>.Update
                .Set(s => s.Name, obj.Name)
                .Set(s => s.TypeList, obj.TypeList);
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
            var filter = Builders<RankingCategory>.Filter.Eq(x=>x.Id, id);
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

        public async Task<long> CountIf(Expression<Func<RankingCategory, bool>> filter)
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
