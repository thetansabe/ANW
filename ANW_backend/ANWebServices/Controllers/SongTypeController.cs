using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANWebServices.Controllers
{
    [Route("api/type")]
    public class SongTypeController : Controller
    {
        // GET: api/<controller>
        private readonly IRepository<SongType> _type;
        private readonly IRepository<Song> _song;
        private readonly IRepository<User> _user;
        private readonly IRepository<Permission> _p;
        public SongTypeController(IRepository<SongType> val, IRepository<User> val2, IRepository<Permission> val3,
            IRepository<Song> song   
            )
        {
            _type = val;
            _user = val2;
            _p = val3;
            _song = song;
        }

        // POST api/<controller>
        [HttpPost]
        [AllowAnonymous]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetAll()
        {
            var json = JsonConvert.SerializeObject(await _type.GetAllAsync());
            return Ok(json);
        }

        [HttpPost("append/{id}")]
        [Authorize(Roles = "10,9")]
        public async Task<IActionResult> Append(string id, [FromBody] SongSubType type)
        {

            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category < 9)
                return Forbid();
            SongType t = await _type.GetById((id));
            if (t == null) return BadRequest();
            if (t.SubType.FindIndex(x => x.Text == type.Text) >= 0)
                return StatusCode(304);
            else
            {
                type.Id = ObjectId.GenerateNewId().ToString();
                t.SubType.Add(type);
                return Ok(JsonConvert.SerializeObject(t));
            }
        }


        [HttpPost("edit")]
        [Authorize(Roles = "10,9")]
        public async Task<IActionResult> Edit([FromQuery] string target, [FromBody] string name)
        {

            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category < 9)
                return Forbid();
            SongType t = await _type.GetById((target));
            if (t == null)
                t = await ((TypeDAO)_type).GetBySubId(target);
            if (t == null) return BadRequest();
            if (t.Id.Equals(target))
                t.Text = name;
            else
            {
                SongSubType sst = t.SubType.Find(x => x.Id.Equals(target));
                sst.Text = name;
            }
            _type.Update((t.Id), t);
            return Ok();
        }


        [HttpPost("create")]
        [Authorize(Roles = "10,9")]
        public async Task<IActionResult> Create([FromBody] SongType type)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category < 9)
                return Forbid();

            type.Id = ObjectId.GenerateNewId().ToString();
            _type.Insert(type);
            return Ok(JsonConvert.SerializeObject(type));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles ="10")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category <= 9)
                return Forbid();

            SongType st = await _type.GetById((id));
            if (st == null)
                st = await ((TypeDAO)_type).GetBySubId(id);
            if (st == null) return NotFound();
            if (st.Id.Equals(id))
            {
                if (st.SubType.Count>0)
                {
                    return Forbid();
                }
                else
                {
                    _type.Delete((st.Id));
                    return Ok();
                }
            }
            else
            {
                SongSubType sst = st.SubType.Find(x => x.Id.Equals(id));
                if (sst == null) return NotFound();
                long count = await ((SongDAO)_song).CountByType(sst.Id);
                if (count == 0)
                {
                    st.SubType.Remove(sst);
                    _type.Update((st.Id), st);
                    return Ok();
                }
                return new StatusCodeResult(409); // Conflict
            }
        }

    }
}
