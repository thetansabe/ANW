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
    public class UserDAO : IRepository<User>
    {
        private readonly DataAccess<User> _dao = null;
        private readonly DataAccess<Song> _song = null;
        private readonly DataAccess<Permission> _perm = null;
        public UserDAO(IOptions<DbSettings> settings)
        {
            _dao = new DataAccess<User>(settings);
            _dao.Context = "user";
            _song = new DataAccess<Song>(settings);
            _song.Context = "song";
            _perm = new DataAccess<Permission>(settings);
            _perm.Context = "permission";
        }
        public async Task<List<User>> Find(Expression<Func<User, bool>> filter)
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


        public async Task<User> FindByUsername(string username)
        {
            try
            {
                return await _dao.obj.Find(x=> x.Username.ToLower()==username.ToLower()).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<List<User>> Find(Expression<Func<User, bool>> filter, int limit, int offset)
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

        public async Task<List<User>> GetAllAsync()
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

        public async Task<List<User>> GetAll(int limit, int offset)
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
        

        public async Task<User> GetById(string id)
        {
            var filter = Builders<User>.Filter.Eq("Id", id);
            try
            {
                return await _dao.obj.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Insert(User obj)
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

        public bool NewUser(string username, string password, string email, string phone, DateTime dob)
        {
            long count = CountUser(username, email).Result;
            if (count > 0) return false;
            User obj = new User()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Username = username,
                Password=password,
                Email=email,
                Phone=phone,
                DateOfBirth=dob,
                DisplayName=username,
                Rank=0,
                MaxChannel=0
            };
            return Insert(obj).Result;
        }

        public async Task<bool> Update(string id, User obj)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var target = Builders<User>.Update
                .Set(s => s.AvatarImg, obj.AvatarImg)
                .Set(s => s.BackgroundImg, obj.BackgroundImg)
                .Set(s => s.CreatedOn, obj.CreatedOn)
                .Set(s => s.DateOfBirth, obj.DateOfBirth)
                .Set(s => s.FullName, obj.FullName)
                .Set(s => s.Desc, obj.Desc)
                .Set(s => s.Email, obj.Email)
                .Set(s => s.Id, obj.Id)
                .Set(s => s.Permission, obj.Permission)
                .Set(s => s.Password, obj.Password)
                .Set(s => s.Phone, obj.Phone)
                .Set(s => s.Username, obj.Username)
                .Set(s => s.Idcard, obj.Idcard)
                .Set(s => s.DisplayName, obj.DisplayName)
                .Set(s => s.Rank, obj.Rank)
                .Set(s => s.MaxChannel, obj.MaxChannel)
                .Set(s => s.Logs, obj.Logs)
                .Set(s => s.SongFavorite, obj.SongFavorite)
                .Set(s => s.VideoFavorite, obj.VideoFavorite)
                .CurrentDate(s => s.UpdatedOn);
            var ref_filter= Builders<Song>.Filter.Eq(x=> x.UploadedBy, id.ToString());
            var ref_target = Builders<Song>.Update.Set(s => s.UploadedBy, obj.Id.ToString());
            try
            {
                UpdateResult result = await _dao.obj.UpdateOneAsync(filter,target);
                await _song.obj.UpdateOneAsync(ref_filter, ref_target);
                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> Delete(string id)
        {
            var filter = Builders<User>.Filter.Eq("Id", id);
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

        public async Task<int> GetPermissionLevel(ObjectId id)
        {
            var filter = Builders<Permission>.Filter.Eq("Id", id);
            try
            {
                Permission p= await _perm.obj.Find(filter).FirstOrDefaultAsync();
                return p.Category;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<long> CountUser(string username, string email)
        {
            List<FilterDefinition<User>> eq = new List<FilterDefinition<User>>();
            var builder = Builders<User>.Filter;
            eq.Add(builder.Regex(x => x.Username, new BsonRegularExpression("/^" + username + "$/i")));
            eq.Add(builder.Regex(x => x.Email, new BsonRegularExpression("/^" + email + "$/i")));
            var filter = Builders<User>.Filter.Or(eq);
            try
            {
                var result = await _dao.obj.Find(filter).CountDocumentsAsync();
                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public async Task<long> CountUsername(string username)
        {
            var filter = Builders<User>.Filter.Regex(x => x.Username, new BsonRegularExpression("/^" + username + "$/i"));
            try
            {
                var result = await _dao.obj.Find(filter).CountDocumentsAsync();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<long> CountUserWithPermission(string permissionId)
        { 
            try
            {
                var result = await _dao.obj.Find(x=>x.Permission==permissionId).CountDocumentsAsync();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<long> CountEmail(string email)
        {
            var filter = Builders<User>.Filter.Regex(x => x.Email, new BsonRegularExpression("/^" + email + "$/i"));
            try
            {
                var result = await _dao.obj.Find(filter).CountDocumentsAsync();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<long> CountIf(Expression<Func<User, bool>> filter)
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
