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
using MongoDB.Driver;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANWebServices.Controllers
{
    [Route("api/ranking")]
    public class RankingController : Controller
    {
        public IRepository<SongRanked> _ranker;
        public IRepository<SongLogs> _log;
        public IRepository<Song> _song;
        public IRepository<User> _user;
        public IRepository<Permission> _p;
        public IRepository<SongType> _type;
        public IRepository<Artist> _art;
        public IRepository<RankingCategory> _rankGrp;
        public IRepository<SystemConfig> _cfg;
        public RankingController(
            IRepository<SongRanked> ranker,
            IRepository<SongLogs> log,
            IRepository<Song> song,
            IRepository<User> user,
            IRepository<Permission> p,
            IRepository<Artist> art,
            IRepository<SongType> type,
            IRepository<SystemConfig> cfg,
            IRepository<RankingCategory> rankGrp
        )
        {
            _ranker = ranker;
            _log = log;
            _song = song;
            _user = user;
            _p = p;
            _art = art;
            _type = type;
            _rankGrp = rankGrp;
            _cfg = cfg;
        }


        public class SummaryResponse {
            public Song song { get; set; }
            public List<SongLogs> summary { get; set; }
        }

        /// <summary>
        /// Return summary of ranking
        /// query result:
        /// - SongId, Song Detail with Views in days
        /// </summary>
        /// <param name="days"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("summary/{days}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 3600*6, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
        public async Task<IActionResult> GetSummary(int days, [FromQuery] int limit=5, [FromQuery] int range = 14)
        {
            SongLogsDAO db = (SongLogsDAO) _log;
            var cfg=await _cfg.GetById(null);
            var type = "";
            if (cfg != null) type = cfg.CategoryToChart;
            dynamic list;
            if (string.IsNullOrEmpty(type))
                list = await db.Context.Aggregate()
                    .Match(x =>
                       x.CreatedOn.CompareTo(DateTime.Now.Subtract(TimeSpan.FromDays(days))) >= 0
                     )
                    .Group(
                        d => d.SongId,
                        group => new
                        {
                            SongId = group.Key,
                            TotalView = group.Sum(g => g.ViewCount)
                        }
                    ).SortByDescending(a => a.TotalView).Limit(limit).ToListAsync();
            else
                list = await db.Context.Aggregate()
                    .Match(x =>
                       x.CreatedOn.CompareTo(DateTime.Now.Subtract(TimeSpan.FromDays(days))) >= 0
                       && x.SongCategory == type
                     )
                    .Group(
                        d => d.SongId,
                        group => new
                        {
                            SongId = group.Key,
                            TotalView = group.Sum(g => g.ViewCount)
                        }
                    ).SortByDescending(a=> a.TotalView).Limit(limit).ToListAsync();
            List<SummaryResponse> result = new List<SummaryResponse>();
            foreach (var item in list)
            {
                string songId = item.SongId;
                var response = new SummaryResponse() {
                    song = await _song.GetById(item.SongId),
                    summary = await _log.Find(x => x.SongId == songId,range,0)
                };
                if (response.song.Artists != null)
                    foreach (string art in response.song.Artists)
                        response.song.ArtistList.Add(await _art.GetById(art));
                result.Add(response);
            }
            if (result.Count == 0) return NoContent();
            return Ok(JsonConvert.SerializeObject(result));
        }

        [HttpGet]
        [Authorize(Roles ="9,10")]
        [ResponseCache(Duration = 10)]
        public async Task<IActionResult> Get([FromQuery] int page, [FromQuery] int size)
        {
            if (page <= 0) return NotFound();
            var userId = HttpContext.User.FindFirst("id").Value.ToString();
            var requester = await _user.GetById(userId);
            if (requester == null) return Unauthorized();
            var permission = await _p.GetById(requester.Permission);
            if (permission == null || permission.Category < 9) return Forbid();
            long count = await _ranker.CountAll();
            var maxPage = Math.Ceiling(count / (double)size);
            if (page > maxPage) return NoContent();
            var start = (page - 1) * size;
            var result = await _ranker.GetAll(size, start);
            foreach (var item in result)
                item.Song = await _song.GetById(item.SongId);
            var json = JsonConvert.SerializeObject(new { list = result, maxPage });
            return Ok(json);
        }


        [HttpGet("type")]
        [Authorize(Roles = "9,10")]
        [ResponseCache(Duration = 10)]
        public async Task<IActionResult> Get([FromQuery] int page, [FromQuery] int size, [FromQuery] string type)
        {
            if (page <= 0) return NotFound();
            var userId = HttpContext.User.FindFirst("id").Value.ToString();
            var requester = await _user.GetById(userId);
            if (requester == null) return Unauthorized();
            var permission = await _p.GetById(requester.Permission);
            if (permission == null || permission.Category < 9) return Forbid();
            long count = await _ranker.CountAll();
            var maxPage = Math.Ceiling(count / (double)size);
            if (page > maxPage) return NoContent();
            var start = (page - 1) * size;
            var result = await _ranker.Find(x=>x.Category.Equals(type),size, start);
            foreach (var item in result)
                item.Song = await _song.GetById(item.SongId);
            var json = JsonConvert.SerializeObject(new { list = result, maxPage });
            return Ok(json);
        }



        [HttpPost]
        [Authorize(Roles = "9,10")]
        [ResponseCache( NoStore =true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> BatchDelete(string[] ids)
        {
            if (!ModelState.IsValid || ids == null) return BadRequest();
            var userId = HttpContext.User.FindFirst("id").Value.ToString();
            var requester = await _user.GetById(userId);
            if (requester == null) return Unauthorized();
            var permission = await _p.GetById(requester.Permission);
            if (permission == null || permission.Category < 9) return Forbid();
            HashSet<string> result = new HashSet<string>();
            foreach (var id in ids)
            {
                var ranker = await _ranker.GetById(id);
                if (ranker != null)
                {
                    _ranker.Delete(id);
                    result.Add(id);
                }
            }
            if (result.Count == 0) return NoContent();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles ="9,10")]
        [ResponseCache(NoStore =true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetRankingInfo(string id)
        {
            if (!ModelState.IsValid) return BadRequest();
            var userId = HttpContext.User.FindFirst("id").Value.ToString();
            var requester = await _user.GetById(userId);
            if (requester == null) return Unauthorized();
            var permission = await _p.GetById(requester.Permission);
            if (permission == null || permission.Category < 9) return Forbid();
            var ranker = await _ranker.GetById(id);
            if (ranker == null) return NoContent();
            return Ok(JsonConvert.SerializeObject(ranker));
        }

        [HttpGet("category")]
        [ResponseCache(Duration =60, Location = ResponseCacheLocation.Any, VaryByHeader = "*")]
        public async Task<IActionResult> GetCategory()
        {
            List<RankingCategory> list = await _rankGrp.GetAllAsync();
            if (list == null || list.Count == 0) return NoContent();
            else return Ok(JsonConvert.SerializeObject(list));
        }

        [HttpPost("category")]
        [Authorize(Roles ="9,10")]
        public async Task<IActionResult> GetCategory([FromBody] RankingCategory model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var userId = HttpContext.User.FindFirst("id").Value.ToString();
            var requester = await _user.GetById(userId);
            if (requester == null) return Unauthorized();
            var permission = await _p.GetById(requester.Permission);
            if (permission == null || permission.Category < 9) return Forbid();

            var gr = new RankingCategory()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = model.Name,
                TypeList = model.TypeList
            };
            _rankGrp.Insert(gr);
            return Ok(JsonConvert.SerializeObject(gr));
        }
        [HttpPut("category")]
        [Authorize(Roles = "9,10")]
        public async Task<IActionResult> SaveCategory([FromBody] RankingCategory model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var userId = HttpContext.User.FindFirst("id").Value.ToString();
            var requester = await _user.GetById(userId);
            if (requester == null) return Unauthorized();
            var permission = await _p.GetById(requester.Permission);
            if (permission == null || permission.Category < 9) return Forbid();

            var grp = await _rankGrp.GetById(model.Id);
            if (grp == null) return BadRequest();

            grp.Name = model.Name;
            grp.TypeList = model.TypeList;

            _rankGrp.Update(grp.Id, grp);
            return Ok(JsonConvert.SerializeObject(grp));
        }
        [HttpDelete("category/{id}")]
        [Authorize(Roles = "9,10")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            if (!ModelState.IsValid) return BadRequest();
            var userId = HttpContext.User.FindFirst("id").Value.ToString();
            var requester = await _user.GetById(userId);
            if (requester == null) return Unauthorized();
            var permission = await _p.GetById(requester.Permission);
            if (permission == null || permission.Category < 9) return Forbid();
            var grp = await _rankGrp.GetById(id);
            if (grp == null) return BadRequest();
            var rankList = await _ranker.Find(x => x.Category == grp.Id);
            foreach (var rank in rankList)
            {
                _log.Delete(rank.SongId);
                _ranker.Delete(rank.SongId);
            }
            _rankGrp.Delete(grp.Id);
            return Ok();
        }

    }
}
