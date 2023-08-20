using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amnhac.Api;
using Amnhac.Interfaces;
using Amnhac.Models;
using ANWebServices.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANWebServices.Controllers
{
    [Route("api/administrator/system")]
    public class SystemController : Controller
    {
        private readonly IRepository<User> _user;
        private readonly IRepository<Permission> _perm;
        private readonly IRepository<SongType> _type;
        private readonly IRepository<Nation> _nation;
        private readonly IRepository<SystemConfig> _cfg;


        public SystemController(IRepository<User> val, 
            IRepository<Permission> val2, 
            IRepository<SongType> val3,
            IRepository<SystemConfig> cfg,
            IRepository<Nation> nation)
        {
            _user = val;
            _perm = val2;
            _type = val3;
            _cfg = cfg;
            _nation = nation;
        }
        // GET: api/<controller>

        [HttpPost("inituser")]
        [AllowAnonymous]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, VaryByHeader ="*")]
        public async Task<IActionResult> CreateAdministrator()
        {
            Permission p = (await _perm.Find(x => x.Category == 10)).FirstOrDefault();
            if (p == null)
            {
                p = new Permission()
                {
                    Id = ObjectId.GenerateNewId().ToString().ToString(),
                    Name = "Administrator",
                    Price = -1,
                    Category = 10,
                    Desc = "Administrator permission"
                };
                _perm.Insert(p);
            }
            User u = (await _user.Find(x => x.Permission == p.Id)).FirstOrDefault();

            if (u == null)
            {
                string key = Startup.secret_key;
                string encodedPass = Convert.ToBase64String(Encoding.UTF8.GetBytes("root:root"));
                string password = CharUtil.AesEnc(CharUtil.PasswordHash(encodedPass, key));
                u = new User()
                {
                    Id = ObjectId.GenerateNewId().ToString().ToString(),
                    Username = "root",
                    Password = password,
                    Desc = "Administrator User",
                    DisplayName = "Administrator",
                    Email = "root@amnhac.com",
                    Permission = p.Id.ToString()
                };
                _user.Insert(u);
                this.CreateTypes();
                return Ok();
            }
            else
            {
                var user = HttpContext.User.FindFirst("id");
                if (user != null)
                {
                    u = await _user.GetById((user.Value));
                    if (u == null) return NotFound();
                    Permission perm = await _perm.GetById((u.Permission));
                    if (perm != null && perm.Category == 10)
                        return await this.CreateTypes();
                    else return NotFound();
                }

                return NotFound();
            }
        }

        [HttpPost("inittype")]
        [Authorize(Roles ="10")]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, VaryByHeader = "*")]
        public async Task<IActionResult> CreateTypes()
        {
            SongType t = (await _type.GetAll(1, 0)).FirstOrDefault();
            if (t == null)
            {

                t = new SongType()
                {
                    Id = ObjectId.GenerateNewId().ToString().ToString(),
                    Text = "Nhạc Việt"
                };
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString().ToString(), Text = "Nhạc Trẻ" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString().ToString(), Text = "Nhạc Trữ tình" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString().ToString(), Text = "Nhạc Trịnh" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString().ToString(), Text = "Nhạc Rap" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString().ToString(), Text = "Nhạc Remix" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString().ToString(), Text = "Nhạc Ballad" });
                await _type.Insert(t);
                t = new SongType()
                {
                    Id = ObjectId.GenerateNewId().ToString().ToString(),
                    Text = "Nhạc ngoại"
                };
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Rock" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Pop" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Đồng quê" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Dance" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Jazz" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Latin" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Nhạc Nhật" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Nhạc Hàn" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Nhạc Hoa" });
                await _type.Insert(t);
                t = new SongType()
                {
                    Id = ObjectId.GenerateNewId().ToString().ToString(),
                    Text = "Thể loại khác"
                };
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Nhạc không lời" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Nhạc phim" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Nhạc nền" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Nhạc điện tử" });
                t.SubType.Add(new SongSubType() { Id = ObjectId.GenerateNewId().ToString(), Text = "Khác" });
                _type.Insert(t);
                this.InitNations();
                return Ok();
            }
            else return this.InitNations();
        }

        [HttpPost("initnation")]
        [Authorize(Roles ="10")]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, VaryByHeader = "*")]
        public ActionResult InitNations()
        {
            if (_nation.CountAll().Result > 0)
                return NotFound();
            string[] nations = { "Việt Nam","Mỹ","Canana","Anh","Thụy Sĩ","Pháp","Hàn Quốc","Nhật Bản",
                "Trung Quốc","Thái Lan","Nga","Úc","Singapore"
            };

            foreach (var nation in nations)
                _nation.Insert(new Nation
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = nation
                });
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles ="10")]
        [ResponseCache(NoStore =true, Location =ResponseCacheLocation.None)]
        public async Task<IActionResult> GetCfg()
        {
            var user = HttpContext.User.FindFirst("id");
            if (user == null) return BadRequest();
            var u = await _user.GetById((user.Value));
            if (u == null) return NotFound();
            Permission perm = await _perm.GetById((u.Permission));
            if (perm == null || perm.Category != 10) return Forbid();

            var result = await _cfg.GetById(null);
            if (result == null)
            {
                var cfg = new SystemConfig();
                _cfg.Insert(cfg);
                result = cfg;
            }
            return Ok(JsonConvert.SerializeObject(result));
        }
        [HttpPut]
        [Authorize(Roles = "10")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> SaveCfg(SystemConfig cfg)
        {
            var user = HttpContext.User.FindFirst("id");
            if (user == null) return BadRequest();
            var u = await _user.GetById((user.Value));
            if (u == null) return NotFound();
            Permission perm = await _perm.GetById((u.Permission));
            if (perm == null || perm.Category != 10) return Forbid();

            var result = await _cfg.GetById(null);
            if (result == null)
            {
                result = new SystemConfig();
                _cfg.Insert(cfg);
            }
            _cfg.Update(null, cfg);
            return Ok(JsonConvert.SerializeObject(cfg));
        }
    }
}
