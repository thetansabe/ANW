using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Amnhac.Api;
using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANWebServices.Controllers
{
    [Route("api/playlist")]
    public class PlaylistController : Controller
    {
        private readonly IRepository<Playlist> _playlist;
        private readonly IRepository<Song> _song;
        private readonly IRepository<User> _user;
        private readonly IRepository<Permission> _p;
        public PlaylistController(IRepository<Playlist> playlist, IRepository<Song> song, IRepository<User> user, IRepository<Permission> p)
        {
            _playlist = playlist;
            _song = song;
            _user = user;
            _p = p;
        }
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("get/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlaylist(string id)
        {
            Playlist p = await _playlist.GetById((id));
            if (p == null)
                return BadRequest();
            Claim claim = HttpContext.User.FindFirst("id");
            if (claim == null && p.Public == 0)
                return NotFound();
            if (claim != null && p.Public == 0)
            {
                string userid = claim.Value;
                if (userid != null)
                {
                    User u = await _user.GetById((userid));
                    if (u!=null)
                    {
                        if (!u.Id.ToString().Equals(p.UserId))
                            return NotFound();
                    }
                    return Unauthorized();
                }
                else return NotFound();
            }
            List<Song> list = new List<Song>();
            if (p.Collection.Count > 0)
            {
                foreach (string sid in p.Collection)
                {
                    list.Add(await _song.GetById((sid)));
                }
            }
            var json = JsonConvert.SerializeObject(new { playlist=p, songlist=list });
            return Ok(json);
        }
        
        [HttpGet("getall")]
        [Authorize]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetMyPlaylist([FromQuery] int page, [FromQuery] int limit)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category <= 8)
            {
                if (page <= 0)
                    page = 1;
                long count = await ((PlaylistDAO)_playlist).CountAll(u.Id.ToString());
                int maxPage = (int)Math.Ceiling(count / (decimal)limit);
                if (page > maxPage) return NotFound();
                int offset = (page - 1) * limit;

                List<Playlist> result = await ((PlaylistDAO)_playlist).FindByUserId(u.Id.ToString(), limit, offset);
                var json = JsonConvert.SerializeObject(new { list= result, currentPage=page, maxPage=maxPage });
                return Ok(json);
            }
            else
            {
                if (page <= 0)
                    page = 1;
                long count = await _playlist.CountAll();
                int maxPage = (int) Math.Ceiling(count/ (decimal) limit);
                if (page > maxPage) return NotFound();
                long offset = (page - 1) * limit;

                List<Playlist> result = await _playlist.GetAll(limit, (page - 1) * limit);
                for (int i=0; i<result.Count; i++)
                {
                    User temp = await _user.GetById((result[i].UserId));
                    if (temp != null)
                        result[i].UserId = temp.Username;
                }
                var json = JsonConvert.SerializeObject(new { list = result, currentPage = page, maxPage = maxPage });
                return Ok(json);
            }
        }

        [HttpGet("getmy/{limit}")]
        [Authorize]
        [ResponseCache(Duration = 180, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetMyPlaylist(int limit)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();

                List<Playlist> result = await ((PlaylistDAO) _playlist).FindByUserId(u.Id.ToString(), limit, 0);
                var json = JsonConvert.SerializeObject(result);
                return Ok(json);
        }

        [HttpPost("find")]
        [AllowAnonymous]
        public async Task<IActionResult> Find([FromBody] string keyword)
        {
            keyword = keyword.Trim();
            List<Playlist> result = await ((PlaylistDAO)_playlist).Filter(keyword.ToLower(),4);
            if (keyword.Contains(" ") && result.Count<=2)
            {
                string[] keys = keyword.ToLower().Split(" ");
                List<Playlist> subResult = await ((PlaylistDAO)_playlist).FilterEach(keys, 4);
                result=result.Union(subResult).ToList();
            }
            var json = JsonConvert.SerializeObject(result);
            return Ok(json);
        }

        [HttpPost("findmy")]
        [Authorize]
        public async Task<IActionResult> FindMy([FromBody] string keyword)
        {
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();

            keyword = keyword.Trim();
            List<Playlist> result = await ((PlaylistDAO)_playlist).Filter(keyword.ToLower(), 4, u.Id.ToString());
            if (keyword.Contains(" ") && result.Count <= 2)
            {
                string[] keys = keyword.ToLower().Split(" ");
                List<Playlist> subResult = await ((PlaylistDAO)_playlist).FilterEach(keys, 4, u.Id.ToString());
                result = result.Union(subResult).ToList();
            }
            var json = JsonConvert.SerializeObject(result);
            return Ok(json);
        }

        [HttpPost("save")]
        [Authorize]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            Console.WriteLine(collection["model"].ToString());
            var model = JsonConvert.DeserializeObject<Playlist>(collection["model"].ToString());
            if (model == null)
                return BadRequest();
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();

            model.Id = ObjectId.GenerateNewId().ToString();
            model.UserId = u.Id.ToString();

            if (model.Tags==null)
                model.Tags = "";
            else
            {
                model.Tags = model.Tags.ToLower() +";";
                model.Tags += CharUtil.ConvertToNonUnicode(model.Tags) + ";";
            }
            model.Tags += model.Name.ToLower() + ";";
            model.Tags += CharUtil.ConvertToNonUnicode(model.Name.ToLower()) + ";";
            bool b =await _playlist.Insert(model);
            if (b)
            {
                var json = JsonConvert.SerializeObject(model);
                return Ok(json);
            }
            return StatusCode(304);
         }

        [HttpPut("save")]
        [Authorize]
        public async Task<IActionResult> Save([FromBody] string body)
        {
            var model = JsonConvert.DeserializeObject<Playlist>(body);
            if (model==null)
                return BadRequest();

            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            Playlist playlist = await _playlist.GetById((model.Id));
            if (p.Category<=8)
            {
                if (!playlist.UserId.Equals(u.Id.ToString()))
                    return Unauthorized();
            }
            playlist.Name = model.Name;
            playlist.ImagePath = model.ImagePath; 
            playlist.Public = model.Public;
            playlist.Collection = model.Collection;
            if (model.Tags == null)
                model.Tags = "";
            else
            {
                model.Tags = model.Tags.ToLower();
                model.Tags += CharUtil.ConvertToNonUnicode(model.Tags);
            }
            model.Tags += model.Name.ToLower();
            model.Tags += CharUtil.ConvertToNonUnicode(model.Name.ToLower());
            playlist.Tags = model.Tags;

            _playlist.Update((playlist.Id), playlist);
            var json = JsonConvert.SerializeObject(playlist);
            return Ok(json);
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            Playlist playlist = await _playlist.GetById( (id));
            if (playlist == null) return NotFound();
            if (p.Category <= 8)
            {
                if (!playlist.UserId.Equals(u.Id.ToString()))
                    return Forbid();
            }

            bool b = await _playlist.Delete((playlist.Id));
            if (b) return Ok();
            return StatusCode(304); // Not modified
        }
    }
}
