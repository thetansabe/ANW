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
    public class SliderDAO : IRepository<Slider>
    {
        private readonly DataAccess<Slider> _dao = null;
        public SliderDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<Slider>(settings);
            _dao.Context = "slider";
        }

        public async Task<List<Slider>> Find(Expression<Func<Slider, bool>> filter)
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

        public async Task<List<Slider>> Find(Expression<Func<Slider, bool>> filter, int limit, int offset)
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
        public async Task<List<Slider>> GetAllAsync()
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

        public async Task<List<Slider>> GetAll(int limit, int offset)
        {
            try
            {
                return await _dao.obj.Find(_=>true).SortByDescending(x=>x.ValidTo).Skip(offset).Limit(limit).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task<List<Slider>> GetAllInRange(DateTime which)
        {
            try
            {
                return await _dao.obj.Find(x=> x.ValidTo>=which && x.ValidFrom<which).SortBy(x=>x.Index).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task<Slider> GetById(string id)
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

        public async Task<bool> Insert(Slider obj)
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

        public async Task<bool> Update(string id, Slider obj)
        {
            var filter = Builders<Slider>.Filter.Eq(x=>x.Id,id);
            UpdateDefinition<Slider> target;
            target = Builders<Slider>.Update
                .Set(s => s.Alignment, obj.Alignment)
                .Set(s => s.Title, obj.Title)
                .Set(s => s.Desc, obj.Desc)
                .Set(s => s.Path, obj.Path)
                .Set(s => s.Goto, obj.Goto)
                .Set(s => s.Index, obj.Index)
                .Set(s => s.ValidFrom, obj.ValidFrom)
                .Set(s => s.ValidTo, obj.ValidTo);
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
            var filter = Builders<Slider>.Filter.Eq(x=>x.Id,id);
            try
            {
                DeleteResult result = await _dao.obj.DeleteOneAsync(filter);
                return result.IsAcknowledged && result.DeletedCount > 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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

        public async Task<long> CountIf(Expression<Func<Slider, bool>> filter)
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
