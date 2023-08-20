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
    public class RoomDAO : IRepository<Room>
    {
        private readonly DataAccess<Room> _dao = null;
        public RoomDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<Room>(settings);
            _dao.Context = "room";
        }
        public async Task<List<Room>> Find(Expression<Func<Room, bool>> filter)
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

        public async Task<List<Room>> Find(Expression<Func<Room, bool>> filter, int limit, int offset)
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



        public async Task<List<Room>> FindRoomHasUser(string userId)
        {
            try
            {
                return await _dao.obj.Find(x=>x.JoinedUsers.Contains(userId)).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<Room> FindByName(string name)
        {
            try
            {
                return await _dao.obj.Find(c=>c.Name.ToLower()==name.ToLower()).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public async Task<List<Room>> GetAllAsync()
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

        public async Task<List<Room>> GetAll(int limit, int offset)
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

        public async Task<Room> GetById(string id)
        {
            var filter = Builders<Room>.Filter.Eq(x=>x.Id, id);
            try
            {
                return await _dao.obj.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(Room obj)
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

        public async Task<bool> Update(string id, Room obj)
        {
            var filter = Builders<Room>.Filter.Eq(x=>x.Id, id);
            var target = Builders<Room>.Update
                .Set(s => s.Name, obj.Name)
                .Set(s => s.Capacity, obj.Capacity)
                .Set(s => s.QueueMode, obj.QueueMode)
                .Set(s => s.AllowChat, obj.AllowChat)
                .Set(s => s.Color, obj.Color)
                .Set(s => s.Icon, obj.Icon)
                .Set(s => s.AllowVoice, obj.AllowVoice)
                .Set(s => s.CreatedBy, obj.CreatedBy)
                .Set(s => s.JoinedUsers, obj.JoinedUsers)
                .Set(s => s.Publicity, obj.Publicity)
                .Set(s => s.ChatLog, obj.ChatLog);
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
            var filter = Builders<Room>.Filter.Eq(x=>x.Id, id);
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

        public async Task<long> CountIf(Expression<Func<Room, bool>> filter)
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
